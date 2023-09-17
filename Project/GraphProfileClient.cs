using Microsoft.Graph;

namespace Project
{
    public class GraphProfileClient
    {
        private readonly ILogger<GraphProfileClient> _logger;
        private readonly GraphServiceClient _graphServiceClient;


        public GraphProfileClient(ILogger<GraphProfileClient> logger, GraphServiceClient graphServiceClient)
        {
            _logger = logger;
            _graphServiceClient = graphServiceClient;
        }
        public async Task<User> GetUserProfile()
        {
            User currentUser = null;

            try
            {
                currentUser = await _graphServiceClient
                    .Me
                    .Request()
                    .Select(u => new
                    {
                        u.DisplayName,
                        u.Mail

                        
                    })
                    .GetAsync();

                return currentUser;
            }
            // Catch CAE exception from Graph SDK
            catch (ServiceException ex) when (ex.Message.Contains("Continuous access evaluation resulted in claims challenge"))
            {
                _logger.LogError($"/me Continuous access evaluation resulted in claims challenge: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"/me Error: {ex.Message}");
                throw;
            }
        }


    }

}

