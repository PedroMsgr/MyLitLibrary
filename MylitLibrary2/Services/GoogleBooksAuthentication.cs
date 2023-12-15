using Google.Apis.Auth.OAuth2;
using Google.Apis.Books.v1;
using Google.Apis.Services;
using System;
using System.IO;

namespace MylitLibrary.Services
{
    class GoogleBooksAuthentication
    {
        private static readonly string[] Scopes = { BooksService.Scope.Books };
        private static readonly string ApplicationName = "MyLitLibraryApp";

        public static BooksService Authenticate()
        {
            try
            {
                var service = new BooksService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = GetCredential(),
                    ApplicationName = ApplicationName,
                });

                return service;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al autenticar: {ex.Message}");
                return null;
            }
        }

        private static ServiceAccountCredential GetCredential()
        {
            var credentialsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "mylitlibraryApiMaster.json");
            try
            {
                var credential = GoogleCredential.FromStream(GetStream(credentialsPath))
                                                  .CreateScoped(Scopes)
                                                  .UnderlyingCredential as ServiceAccountCredential;
                return credential;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener las credenciales: {ex.Message}");
                return null;
            }
        }
        private static Stream GetStream(string path)
        {
            return new FileStream(path, FileMode.Open, FileAccess.Read);
        }
    }
}

