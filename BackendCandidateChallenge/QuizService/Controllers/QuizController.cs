using System.Collections.Generic;
using System.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using QuizService.Model;
using System.Linq;
using QuizService.Repository;
using System.Threading.Tasks;

namespace QuizService.Controllers;

[Route("api/quizzes")]
//TODO: I would inherit from IAsyncActionFilter to add handle exceptions generally by implementing method OnActionExecutionAsync
public class QuizController : Controller
{
    //TODO: I would remove IDbConnection and would add all database connections to service IQuizRepository

    private readonly IDbConnection _connection;
    private readonly IQuizRepository _quizRepo;

    public QuizController(IDbConnection connection, IQuizRepository quizRepo)
    {
        _connection = connection;
        _quizRepo = quizRepo;
    }

    // GET api/quizzes
    [HttpGet]
    public async Task<IEnumerable<QuizResponseModel>> Get() {
        return (await _quizRepo.GetAllAsync()).Select(quiz => new QuizResponseModel
        {
            Id = quiz.Id,
            Title = quiz.Title
        });
    }

    // GET api/quizzes/5
    [HttpGet("{id}")]
    public async Task<object> Get(int id)
    {
        Dictionary<string, string> links = new Dictionary<string, string>
                {
                    {"self", $"/api/quizzes/{id}"},
                    {"questions", $"/api/quizzes/{id}/questions"}
                };

        QuizResponseModel quizResponse = (await _quizRepo.GetQuizWithAsnwer(id)).GroupBy(quiz=>quiz.Id).
            Select(quiz => new QuizResponseModel { 
                Id = quiz.Key,
                Title = quiz.Select(t=>t.Title).FirstOrDefault(),
                Links = links,
                Questions = quiz.Any(question => question.QuestionId != null)? quiz.GroupBy(question=>question.QuestionId).
                Select(question=>new QuizResponseModel.QuestionItem { 
                    Id = question.Key.Value,
                    Text = question.Select(t=>t.Text).FirstOrDefault(),
                    CorrectAnswerId = question.Select(t => t.CorrectAnswerId).FirstOrDefault(),
                    Answers = question.Any(answer => answer.AnswerId != null) ? question.Select(answer => new QuizResponseModel.AnswerItem { 
                        Text = answer.AnswerText,
                        Id = answer.AnswerId.Value
                    }): new List<QuizResponseModel.AnswerItem>()
                }):new List<QuizResponseModel.QuestionItem>()
            }).FirstOrDefault();
        
        return quizResponse == null ? NotFound() : quizResponse;
    }

    // POST api/quizzes
    [HttpPost]
    public IActionResult Post([FromBody]QuizCreateModel value)
    {
        //TODO: I would add code if (!ModelState.IsValid){} to validate inputs.
        var sql = $"INSERT INTO Quiz (Title) VALUES('{value.Title}'); SELECT LAST_INSERT_ROWID();";
        var id = _connection.ExecuteScalar(sql);
        return Created($"/api/quizzes/{id}", null);
    }

    // PUT api/quizzes/5
    [HttpPut("{id}")]
    public IActionResult Put(int id, [FromBody]QuizUpdateModel value)
    {
        //TODO: I would add code: if (!ModelState.IsValid){} to validate inputs.
        const string sql = "UPDATE Quiz SET Title = @Title WHERE Id = @Id";
        int rowsUpdated = _connection.Execute(sql, new {Id = id, Title = value.Title});
        if (rowsUpdated == 0)
            return NotFound();
        return NoContent();
    }

    // DELETE api/quizzes/5
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        const string sql = "DELETE FROM Quiz WHERE Id = @Id";
        int rowsDeleted = _connection.Execute(sql, new {Id = id});
        if (rowsDeleted == 0)
            return NotFound();
        return NoContent();
    }

    // POST api/quizzes/5/questions
    [HttpPost]
    [Route("{id}/questions")]
    public IActionResult PostQuestion(int id, [FromBody]QuestionCreateModel value)
    {
        //TODO: I would add code: if (!ModelState.IsValid){} to validate inputs.
        const string sql = "INSERT INTO Question (Text, QuizId) VALUES(@Text, @QuizId); SELECT LAST_INSERT_ROWID();";
        //TODO: I would add try{} catch block here. There is need a try catch exception handler in order to deal with cases where there is foreign key constraint violated.  
        var questionId = _connection.ExecuteScalar(sql, new {Text = value.Text, QuizId = id});
        return Created($"/api/quizzes/{id}/questions/{questionId}", null);
    }

    // PUT api/quizzes/5/questions/6
    [HttpPut("{id}/questions/{qid}")]
    public IActionResult PutQuestion(int id, int qid, [FromBody]QuestionUpdateModel value)
    {
        //TODO: I would add code: if (!ModelState.IsValid){} to validate inputs.

        const string sql = "UPDATE Question SET Text = @Text, CorrectAnswerId = @CorrectAnswerId WHERE Id = @QuestionId";
        int rowsUpdated = _connection.Execute(sql, new {QuestionId = qid, Text = value.Text, CorrectAnswerId = value.CorrectAnswerId});
        if (rowsUpdated == 0)
            return NotFound();
        return NoContent();
    }

    // DELETE api/quizzes/5/questions/6
    [HttpDelete]
    [Route("{id}/questions/{qid}")]
    public IActionResult DeleteQuestion(int id, int qid)
    {
        const string sql = "DELETE FROM Question WHERE Id = @QuestionId";
        //TODO: I would check if number of rows affected is greater than zero.
        _connection.ExecuteScalar(sql, new {QuestionId = qid});
        return NoContent();
    }

    // POST api/quizzes/5/questions/6/answers
    [HttpPost]
    [Route("{id}/questions/{qid}/answers")]
    public IActionResult PostAnswer(int id, int qid, [FromBody]AnswerCreateModel value)
    {
        //TODO: I would add code: if (!ModelState.IsValid){} to validate inputs.
        const string sql = "INSERT INTO Answer (Text, QuestionId) VALUES(@Text, @QuestionId); SELECT LAST_INSERT_ROWID();";
        //TODO: I would add try{} catch block here. There is need a try catch exception handler in order to deal with cases where there is foreign key constraint violated.  
        var answerId = _connection.ExecuteScalar(sql, new {Text = value.Text, QuestionId = qid});
        return Created($"/api/quizzes/{id}/questions/{qid}/answers/{answerId}", null);
    }

    // PUT api/quizzes/5/questions/6/answers/7
    [HttpPut("{id}/questions/{qid}/answers/{aid}")]
    public IActionResult PutAnswer(int id, int qid, int aid, [FromBody]AnswerUpdateModel value)
    {
        //TODO: I would add code: if (!ModelState.IsValid){} to validate inputs.
        const string sql = "UPDATE Answer SET Text = @Text WHERE Id = @AnswerId";
        int rowsUpdated = _connection.Execute(sql, new {AnswerId = qid, Text = value.Text});
        if (rowsUpdated == 0)
            return NotFound();
        return NoContent();
    }

    // DELETE api/quizzes/5/questions/6/answers/7
    [HttpDelete]
    [Route("{id}/questions/{qid}/answers/{aid}")]
    public IActionResult DeleteAnswer(int id, int qid, int aid)
    {
        const string sql = "DELETE FROM Answer WHERE Id = @AnswerId";
        _connection.ExecuteScalar(sql, new {AnswerId = aid});
        //TODO: I would check if number of rows affected is greater than zero.
        return NoContent();
    }
}