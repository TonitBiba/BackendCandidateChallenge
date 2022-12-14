using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using QuizService.Model;
using Xunit;

namespace QuizService.Tests;

public class QuizzesControllerTest
{
    const string QuizApiEndPoint = "/api/quizzes/";

    [Fact]
    public async Task PostNewQuizAddsQuiz()
    {
        var quiz = new QuizCreateModel("Test title");
        using (var testHost = new TestServer(new WebHostBuilder()
                   .UseStartup<Startup>()))
        {
            var client = testHost.CreateClient();
            var content = new StringContent(JsonConvert.SerializeObject(quiz));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var response = await client.PostAsync(new Uri(testHost.BaseAddress, $"{QuizApiEndPoint}"),
                content);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(response.Headers.Location);
        }
    }

    [Fact]
    public async Task AQuizExistGetReturnsQuiz()
    {
        using (var testHost = new TestServer(new WebHostBuilder()
                   .UseStartup<Startup>()))
        {
            var client = testHost.CreateClient();
            const long quizId = 1;
            var response = await client.GetAsync(new Uri(testHost.BaseAddress, $"{QuizApiEndPoint}{quizId}"));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            var quiz = JsonConvert.DeserializeObject<QuizResponseModel>(await response.Content.ReadAsStringAsync());
            Assert.Equal(quizId, quiz.Id);
            Assert.Equal("My first quiz", quiz.Title);
        }
    }

    [Fact]
    public async Task AQuizDoesNotExistGetFails()
    {
        using (var testHost = new TestServer(new WebHostBuilder()
                   .UseStartup<Startup>()))
        {
            var client = testHost.CreateClient();
            const long quizId = 999;
            var response = await client.GetAsync(new Uri(testHost.BaseAddress, $"{QuizApiEndPoint}{quizId}"));
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }

    [Fact]
        
    public async Task AQuizDoesNotExists_WhenPostingAQuestion_ReturnsNotFound()
    {
        const string QuizApiEndPoint = "/api/quizzes/999/questions";

        using (var testHost = new TestServer(new WebHostBuilder()
                   .UseStartup<Startup>()))
        {
            var client = testHost.CreateClient();
            const long quizId = 999;
            var question = new QuestionCreateModel("The answer to everything is what?");
            var content = new StringContent(JsonConvert.SerializeObject(question));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var response = await client.PostAsync(new Uri(testHost.BaseAddress, $"{QuizApiEndPoint}"),content);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }

    [Fact]
    public async Task QuizWithTwoQuestionTwoAnswer() {
        const string QuizApiEndPoint = "/api/quizzes";
        string quizUrl, questionUrl;
        int quizId = 0;
        
        using (var testHost = new TestServer(new WebHostBuilder()
                          .UseStartup<Startup>()))
        {
            HttpClient client = testHost.CreateClient();
            StringContent content = new StringContent(JsonConvert.SerializeObject(new { Title = "New quiz" }));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            HttpResponseMessage response = await client.PostAsync(new Uri(testHost.BaseAddress, $"{QuizApiEndPoint}"), content);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(response.Headers.Location);

            quizUrl = response.Headers.Location.OriginalString;
            string[] quizLocationResponse = response.Headers.Location.OriginalString.Split("/");
            quizId = int.Parse(quizLocationResponse[quizLocationResponse.Length - 1]);
            quizUrl += "/questions";

            for (int i = 0; i < 2; i++)
            {
                content = new StringContent(JsonConvert.SerializeObject(new { Text = $"Question {i+1}" }));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                response = await client.PostAsync(new Uri(testHost.BaseAddress, $"{quizUrl}"), content);

                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                Assert.NotNull(response.Headers.Location);
                questionUrl = response.Headers.Location.OriginalString;
                int correctAsnwerID = 0;
                for(int j = 0; j < 2; j++)
                {
                    content = new StringContent(JsonConvert.SerializeObject(new { Text = $"Answer {j+1}" }));
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    response = await client.PostAsync(new Uri(testHost.BaseAddress, $"{questionUrl + "/answers"}"), content);
                    string[] answerAocationResponse = response.Headers.Location.OriginalString.Split("/");
                    correctAsnwerID =  int.Parse(answerAocationResponse[answerAocationResponse.Length - 1]);

                    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                    Assert.NotNull(response.Headers.Location);
                }

                content = new StringContent(JsonConvert.SerializeObject(new { CorrectAnswerId = correctAsnwerID, Text = $"Question {i+1}" }));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                response = await client.PutAsync(new Uri(testHost.BaseAddress, $"{questionUrl}"), content);

                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }

            response = await client.GetAsync(new Uri(testHost.BaseAddress, $"{QuizApiEndPoint}/{quizId}"));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);
            var quiz = JsonConvert.DeserializeObject<QuizResponseModel>(await response.Content.ReadAsStringAsync());

        }
    }
}