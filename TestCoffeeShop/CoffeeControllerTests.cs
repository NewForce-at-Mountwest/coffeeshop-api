using CoffeeShop.Models;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Collections.Generic;


namespace CoffeeShop.Tests
{
    public class CoffeeControllerTest
    {

        // This is going to be our test coffee instance that we create and delete to make sure everything works
        private Coffee dummyCoffee { get; } = new Coffee
        {
            BeanType = "Kenyan",
            Title = "Latte"
        };

        // We'll store our base url for this route as a private field to avoid typos
        private string url { get; } = "/api/coffees";


        // Reusable method to create a new coffee in the database and return it
        public async Task<Coffee> CreateDummyCoffee()
        {

            using (var client = new APIClientProvider().Client)
            {

                // Serialize the C# object into a JSON string
                string frenchRoastAsJSON = JsonConvert.SerializeObject(dummyCoffee);


                // Use the client to send the request and store the response
                HttpResponseMessage response = await client.PostAsync(
                    url,
                    new StringContent(frenchRoastAsJSON, Encoding.UTF8, "application/json")
                );

                // Store the JSON body of the response
                string responseBody = await response.Content.ReadAsStringAsync();

                // Deserialize the JSON into an instance of Coffee
                Coffee newlyCreatedCoffee = JsonConvert.DeserializeObject<Coffee>(responseBody);

                return newlyCreatedCoffee;
            }
        }

        // Reusable method to deelte a coffee from the database
        public async Task deleteDummyCoffee(Coffee coffeeToDelete)
        {
            using (HttpClient client = new APIClientProvider().Client)
            {
                HttpResponseMessage deleteResponse = await client.DeleteAsync($"{url}/{coffeeToDelete.Id}");

            }

        }


        /* TESTS START HERE */


        [Fact]
        public async Task Create_Coffee()
        {
            using (var client = new APIClientProvider().Client)
            {
                // Create a new coffee in the db
                Coffee newFrenchRoast = await CreateDummyCoffee();

                // Try to get it again
                HttpResponseMessage response = await client.GetAsync($"{url}/{newFrenchRoast.Id}");
                response.EnsureSuccessStatusCode();

                // Turn the response into JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Turn the JSON into C#
                Coffee newCoffee = JsonConvert.DeserializeObject<Coffee>(responseBody);

                // Make sure it's really there
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(dummyCoffee.Title, newCoffee.Title);
                Assert.Equal(dummyCoffee.BeanType, newCoffee.BeanType);

                // Clean up after ourselves
                await deleteDummyCoffee(newCoffee);

            }

        }


        [Fact]

        public async Task Delete_Coffee()
        {
            // Note: with many of these methods, I'm creating dummy data and then testing to see if I can delete it. I'd rather do that for now than delete something else I (or a user) created in the database, but it's not essential-- we could test deleting anything 

            // Create a new coffee in the db
            Coffee newFrenchRoast = await CreateDummyCoffee();

            // Delete it
            await deleteDummyCoffee(newFrenchRoast);

            using (var client = new APIClientProvider().Client)
            {
                // Try to get it again
                HttpResponseMessage response = await client.GetAsync($"{url}{newFrenchRoast.Id}");

                // Make sure it's really gone
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            }
        }

        [Fact]
        public async Task Get_All_Coffee()
        {

            using (var client = new APIClientProvider().Client)
            {

                // Try to get all of the coffees from /api/coffees
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                // Convert to JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Convert from JSON to C#
                List<Coffee> coffees = JsonConvert.DeserializeObject<List<Coffee>>(responseBody);

                // Make sure we got back a 200 OK Status and that there are more than 0 coffees in our database
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(coffees.Count > 0);

            }
        }

        [Fact]
        public async Task Get_Single_Coffee()
        {
            using (HttpClient client = new APIClientProvider().Client)
            {
                // Create a dummy coffee
                Coffee newFrenchRoast = await CreateDummyCoffee();

                // Try to get it
                HttpResponseMessage response = await client.GetAsync($"{url}/{newFrenchRoast.Id}");
                response.EnsureSuccessStatusCode();

                // Turn the response into JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Turn the JSON into C#
                Coffee frenchRoastFromDB = JsonConvert.DeserializeObject<Coffee>(responseBody);

                // Did we get back what we expected to get back? 
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(dummyCoffee.Title, frenchRoastFromDB.Title);
                Assert.Equal(dummyCoffee.BeanType, frenchRoastFromDB.BeanType);

                // Clean up after ourselves-- delete the dummy coffee we just created
                await deleteDummyCoffee(frenchRoastFromDB);

            }
        }




        [Fact]
        public async Task Update_Coffee()
        {

            using (var client = new APIClientProvider().Client)
            {
                // Create a dummy coffee
                Coffee newFrenchRoast = await CreateDummyCoffee();

                // Make a new title and assign it to our dummy coffee
                string newTitle = "FRENCHY MCFRENCH ROAST";
                newFrenchRoast.Title = newTitle;

                // Convert it to JSON
                string modifiedFrenchRoastAsJSON = JsonConvert.SerializeObject(newFrenchRoast);

                // Try to PUT the newly edited coffee
                var response = await client.PutAsync(
                    $"{url}/{newFrenchRoast.Id}",
                    new StringContent(modifiedFrenchRoastAsJSON, Encoding.UTF8, "application/json")
                );

                // See what comes back from the PUT. Is it a 204? 
                string responseBody = await response.Content.ReadAsStringAsync();
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                // Get the edited coffee back from the database after the PUT
                var getModifiedCoffee = await client.GetAsync($"{url}/{newFrenchRoast.Id}");
                getModifiedCoffee.EnsureSuccessStatusCode();

                // Convert it to JSON
                string getCoffeeBody = await getModifiedCoffee.Content.ReadAsStringAsync();

                // Convert it from JSON to C#
                Coffee newlyEditedCoffee = JsonConvert.DeserializeObject<Coffee>(getCoffeeBody);

                // Make sure the title was modified correctly
                Assert.Equal(HttpStatusCode.OK, getModifiedCoffee.StatusCode);
                Assert.Equal(newTitle, newlyEditedCoffee.Title);

                // Clean up after yourself
                await deleteDummyCoffee(newlyEditedCoffee);
            }
        }

        [Fact]
        public async Task Test_Get_NonExitant_Coffee_Fails()
        {

            using (var client = new APIClientProvider().Client)
            {
                // Try to get a coffee with an Id that could never exist
                HttpResponseMessage response = await client.GetAsync($"{url}/00000000");

                // It should bring back a 204 no content error
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }
        }

        [Fact]
        public async Task Test_Delete_NonExistent_Coffee_Fails()
        {
            using (var client = new APIClientProvider().Client)
            {
                // Try to delete an Id that shouldn't exist 
                HttpResponseMessage deleteResponse = await client.DeleteAsync($"{url}0000000000");

                Assert.False(deleteResponse.IsSuccessStatusCode);
                Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
            }
        }
    }
}




