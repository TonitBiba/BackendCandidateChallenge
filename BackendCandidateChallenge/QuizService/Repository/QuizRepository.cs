namespace QuizService.Repository {
    using Dapper;
    using QuizService.Model;
    using QuizService.Model.Domain;
    using System.Collections.Generic;
    using System.Data;
    using System.Threading.Tasks;

    public class QuizRepository : IQuizRepository {
        
        private readonly IDbConnection _connection;

        public QuizRepository(IDbConnection _connection) {
            this._connection = _connection;
        }

        public async Task<IEnumerable<Quiz>> GetAllAsync() {
            const string sql = "SELECT * FROM Quiz;";
            var quizzes = await _connection.QueryAsync<Quiz>(sql);
            return quizzes;
        }

        public async Task<IEnumerable<QuizWithAnswerModel>> GetQuizWithAsnwer(int id) {
            const string sql = "SELECT Quiz.Id, Quiz.Title, Question.Id AS QuestionId, Question.Text, Question.CorrectAnswerId, Answer.Id AS AnswerId, Answer.Text as AnswerText " +
                               "FROM Quiz " +
                               "LEFT JOIN Question ON Question.QuizId = Quiz.Id " +
                               "LEFT JOIN Answer ON Answer.QuestionId = Question.Id " +
                               "WHERE Quiz.Id=@Id;";
            var quizWithAnswer = await _connection.QueryAsync<QuizWithAnswerModel>(sql, new { Id = id });
            return quizWithAnswer;
        }
    }
}
