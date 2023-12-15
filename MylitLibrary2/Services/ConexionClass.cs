using MylitLibrary.Entities;
using Microsoft.EntityFrameworkCore;
using MylitLibrary.Data;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic.Logging;

namespace MylitLibrary.Services
{
    class ConexionClass
    {

        public ConexionClass()
        {
        }

        // Verifica si un libro existe para un usuario específico.
        public async Task<bool> ExistsBook(int idUsuario, string bookId)
        {
            // Patron Using asegura que el contexto se dispone correctamente.
            using (var context = new LibraryDatacontexContext(LibraryDatacontexContext.BuildDbContextOptions()))
            {
                // Utiliza LINQ para consultar la base de datos de manera asincrónica
                return await context.UserBooks.AnyAsync(ub => ub.IdUser == idUsuario && ub.IdBook == bookId);
            }
        }



        // Actualiza los detalles de un libro específico para un usuario.
        public async Task UpdateBook(int idUsuario, string bookId, string titleBook,
            string authorBook, string datePublicBook, string coverUrl, string estado, bool esFavorito, string comentario, int? nota)
        {
            // Verificar que el usuario y el libro existen
            if (idUsuario <= 0)
                return;
            // Patron Using
            using (var context = new LibraryDatacontexContext(LibraryDatacontexContext.BuildDbContextOptions()))
            {
                // Encuentra el libro basado en la clave compuesta de usuario y libro.
                var bookToUpdate = await context.UserBooks.FindAsync(idUsuario, bookId);
                if (bookToUpdate != null)
                {
                    // Actualiza las propiedades del libro
                    bookToUpdate.TitleBook = titleBook;
                    bookToUpdate.Author = authorBook;
                    bookToUpdate.DatePublicBook = datePublicBook;
                    bookToUpdate.CoverUrl = coverUrl;
                    bookToUpdate.StatusBook = estado;
                    bookToUpdate.Favorite = esFavorito;
                    bookToUpdate.Coments = comentario;
                    bookToUpdate.NoteOfBook = nota;

                    // Guardar los cambios en la base de datos
                    await context.SaveChangesAsync();
                }
            }
        }


        // Guarda un nuevo libro en la base de datos para un usuario.
        public async Task SaveBook(int idUsuario, string bookId, string title, string author,
                           string datePublicBook, string coverUrl, string estado, bool esFavorito,
                           string comentario, int? nota)
        {
            // Verifica
            if (idUsuario <= 0)
                return;
            // Patron Using
            using (var context = new LibraryDatacontexContext(LibraryDatacontexContext.BuildDbContextOptions()))
            {
                var newBook = new UserBooks
                {
                    IdUser = idUsuario,
                    IdBook = bookId,
                    TitleBook = title,
                    Author = author,
                    DatePublicBook = datePublicBook,
                    CoverUrl = coverUrl,
                    StatusBook = estado,
                    Favorite = esFavorito,
                    Coments = comentario,
                    NoteOfBook = nota
                };

                context.UserBooks.Add(newBook);
                // Guarda los cambios de la base de datos
                await context.SaveChangesAsync();
            }
        }

        // Elimina un libro de la lista de un usuario.
        public async Task<bool> DeleteBook(int userId, string bookId)
        {
            // Patron Using
            using (var context = new LibraryDatacontexContext(LibraryDatacontexContext.BuildDbContextOptions()))
            {
                // Encuentra el libro y, si existe, lo elimina del contexto y guarda los cambios.
                var book = await context.UserBooks
                                     .FirstOrDefaultAsync(ub => ub.IdUser == userId && ub.IdBook == bookId);

                if (book != null)
                {
                    context.UserBooks.Remove(book);
                    await context.SaveChangesAsync();
                    return true;
                }

                return false;
            }
        }

        // Verifica si un usuario existe por su nombre de usuario.
        public async Task<bool> ExistsUserByUserName(string username)
        {
            // Verifica no  Nulo
            if (string.IsNullOrEmpty(username))
                return false;
            // Patron Using
            using (var context = new LibraryDatacontexContext(LibraryDatacontexContext.BuildDbContextOptions()))
            {
                return await context.UserList.AnyAsync(u => u.UserName == username);
            }
        }

        // Verifica si un usuario existe por su ID.
        public async Task<bool> ExistsUserById(int userId)
        {
            if (userId <= 0)
                return false;
            // Patron Using
            using (var context = new LibraryDatacontexContext(LibraryDatacontexContext.BuildDbContextOptions()))
            {
                return await context.UserList.AnyAsync(u => u.Id == userId);
            }
        }



        //Genera el patron de seguridad de la contraseña
        private string HashPassword(string password)
        {
            int workFactor = 10;

            // Generar el hash de la contraseña
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor);
        }


        //Verifica la contraseña
        private bool VerifyPasswordHash(string password, string storedHash)
        {
            // Verificar el hash de la contraseña ingresada con el hash almacenado
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }


        // Verifica las credenciales de un usuario.
        public async Task<bool> VerifyCredentials(string username, string password)
        {
            // Verifica no Nulos
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return false;
            // Patron Using
            using (var context = new LibraryDatacontexContext(LibraryDatacontexContext.BuildDbContextOptions()))
            {
                var user = await context.UserList
                                     .Where(u => u.UserName == username)
                                     .Select(u => u.Password)
                                     .FirstOrDefaultAsync();

                if (user == null)
                    return false;

                return VerifyPasswordHash(password, user);
            }
        }


        // Registra un nuevo usuario en la base de datos.
        public async Task RegisterUser(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return;
            // Patron Using
            using (var context = new LibraryDatacontexContext(LibraryDatacontexContext.BuildDbContextOptions()))
            {
                // Patron de contraseña
                string passwordHash = HashPassword(password);

                var newUser = new UserList
                {
                    UserName = username,
                    Password = passwordHash
                };

                context.UserList.Add(newUser);
                await context.SaveChangesAsync();
            }
        }

        // Obtiene el ID de usuario basado en el nombre de usuario.
        public async Task<int?> GetUserIdByUsername(string username)
        {
            if (string.IsNullOrEmpty(username))
                return null;
            // Patron Using
            using (var context = new LibraryDatacontexContext(LibraryDatacontexContext.BuildDbContextOptions()))
            {
                var user = await context.UserList
                                     .Where(u => u.UserName == username)
                                     .Select(u => u.Id)
                                     .FirstOrDefaultAsync();

                return user == 0 ? (int?)null : user;
            }
        }

        // Obtiene el nombre de usuario basado en su ID.
        public async Task<string> GetUsernameById(int userId)
        {
            // Patron Using
            using (var context = new LibraryDatacontexContext(LibraryDatacontexContext.BuildDbContextOptions()))
            {
                var user = await context.UserList
                                     .Where(u => u.Id == userId)
                                     .Select(u => u.UserName)
                                     .FirstOrDefaultAsync();

                return user;
            }
        }


        // Obtiene los datos de un libro para un usuario específico.
        public async Task<UserBooks> GetBookDataForUser(int userId, string bookId)
        {
            if (userId <= 0)
                return null;
            // Patron Using
            using (var context = new LibraryDatacontexContext(LibraryDatacontexContext.BuildDbContextOptions()))
            {
                // Consulta los detalles del libro basado en el usuario y el ID del libro.
                return await context.UserBooks
                                 .Where(ub => ub.IdUser == userId && ub.IdBook == bookId)
                                 .FirstOrDefaultAsync();
            }
        }


        // Obtiene todos los libros de un usuario.
        public async Task<List<UserBooks>> GetBooksByUser(int userId)
        {
            // Patron Using
            using (var context = new LibraryDatacontexContext(LibraryDatacontexContext.BuildDbContextOptions()))
            {
                // Consulta y devuelve todos los libros de un usuario.
                return await context.UserBooks
                                 .Where(ub => ub.IdUser == userId)
                                 .ToListAsync();
            }
        }

    }

}
