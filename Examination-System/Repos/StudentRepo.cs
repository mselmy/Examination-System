﻿using Examination_System.Data;
using Examination_System.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System.ComponentModel;

namespace Examination_System.Repos
{
    public interface IStudentRepo
    {
        public Task<Exam> GetExamByCrsId(int crsId);
        public Task<Exam> GetExamById(int examId);
        public Task<bool> SubmitExam(int examId, int studentId, List<StudentAnswer> studentAnswers);
        public Task<StudentExam> GetStudentExamDegree(int examId, int studentId);
        public Task<bool> IsStudentExamSubmitted(int examId, int studentId);

    }

    public class StudentRepo : IStudentRepo
    {
        readonly ExaminationSystemContext db;

        public StudentRepo(ExaminationSystemContext _db)
        {
            db = _db;
        }

        public async Task<Exam> GetExamByCrsId(int crsId) //get exam by course id
        {
            try
            {
                return await db.Exams.Where(e => e.CrsId == crsId).Include(c => c.Crs).Include(e => e.ExamQuestions).ThenInclude(q => q.Question).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<Exam> GetExamById(int examId) //get exam by id
        {
            try
            {
                return await db.Exams.Where(e => e.ExamId == examId).Include(c => c.Crs).Include(e => e.ExamQuestions).ThenInclude(q => q.Question).ThenInclude(qo => qo.QuestionOptions).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<bool> SubmitExam(int examId, int studentId , List<StudentAnswer> studentAnswers) //submit the exam
        {
            int grade = 0;
            //float finalGrade = db.ExamQuestions.Where(eq => eq.ExamId == studentAnswers[0].ExamId).Sum(eq => eq.Degree); //get the total degree of the exam
            
            try
            {
                foreach (var answer in studentAnswers)
                {
                    var questionGrade = await db.ExamQuestions.Where(eq => eq.ExamId == answer.ExamId && eq.QuestionId == answer.QuestionId).Select(eq => eq.Degree).FirstOrDefaultAsync(); //get the degree of the question
                    if(answer.SelectedOption == 1)
                    {
                        grade += questionGrade; //add the degree of the question to the total grade
                    }
                    db.StudentAnswers.Add(answer);
                }

                await db.SaveChangesAsync();


                StudentExam studentExam = new StudentExam
                {
                    ExamId = examId,
                    StdId = studentId,
                    ExamDate = DateOnly.FromDateTime(DateTime.Now),
                    Grade = grade
                };

                db.StudentExams.Add(studentExam);

                await db.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        
        public async Task<StudentExam> GetStudentExamDegree(int examId, int studentId) //get the student exam degree
        {
            try
            {
                return await db.StudentExams.Where(se => se.ExamId == examId && se.StdId == studentId).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<bool> IsStudentExamSubmitted(int examId, int studentId) //check if the student exam is submitted
        {
            try
            {
                return await db.StudentExams.AnyAsync(se => se.ExamId == examId && se.StdId == studentId);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
