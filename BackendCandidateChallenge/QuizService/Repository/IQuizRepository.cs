namespace QuizService.Repository {
    using QuizService.Model;
    using QuizService.Model.Domain;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IQuizRepository {
        
        Task<IEnumerable<Quiz>> GetAllAsync();

        Task<IEnumerable<QuizWithAnswerModel>> GetQuizWithAsnwer(int id);

    }
}