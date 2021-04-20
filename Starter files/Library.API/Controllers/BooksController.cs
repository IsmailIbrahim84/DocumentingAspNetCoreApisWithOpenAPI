using AutoMapper;
using Library.API.Models;
using Library.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Library.API.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Library.API.Controllers
{
    [Route("api/authors/{authorId}/books")]
    [ApiController]
    public class BooksController : ControllerBase
    { 
        private readonly IBookRepository _bookRepository;
        private readonly IAuthorRepository _authorRepository;
        private readonly IMapper _mapper;

        public BooksController(
            IBookRepository bookRepository,
            IAuthorRepository authorRepository,
            IMapper mapper)
        {
            _bookRepository = bookRepository;
            _authorRepository = authorRepository;
            _mapper = mapper;
        }
       /// <summary>
       /// Get a book by Author Id 
       /// </summary>
       /// <param name="authorId">The id of author.</param>
       /// <returns>An action result of type book.</returns>
       /// <response code="200">Returns the requested book</response>
       [ProducesResponseType(StatusCodes.Status404NotFound)]
       [ProducesResponseType(StatusCodes.Status400BadRequest)]
       [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet()]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooks(
        Guid authorId )
        {
            if (!await _authorRepository.AuthorExistsAsync(authorId))
            {
                return NotFound();
            }

            var booksFromRepo = await _bookRepository.GetBooksAsync(authorId); 
            return Ok(_mapper.Map<IEnumerable<Book>>(booksFromRepo));
        }


       /// <summary>
       /// Get the book by Book Id and Author Id.
       /// </summary>
       /// <param name="authorId">The Author Id.</param>
       /// <param name="bookId">The book Id</param>
       /// <returns>Ac action result type of book with Author first name and last name.</returns>
       [ProducesResponseType(StatusCodes.Status404NotFound)]
       [ProducesResponseType(StatusCodes.Status400BadRequest)]
       [ProducesResponseType(StatusCodes.Status200OK)]
       [Produces("application/json","application/xml")]
       [RequestHeaderMatchesMediaType(HeaderNames.Accept,"application/json","application/vnd.marvin.book+json")]
        [HttpGet("{bookId}")]
        public async Task<ActionResult<Book>> GetBook(
            Guid authorId,
            Guid bookId)
        {
            if (! await _authorRepository.AuthorExistsAsync(authorId))
            {
                return NotFound();
            }

            var bookFromRepo = await _bookRepository.GetBookAsync(authorId, bookId);
            if (bookFromRepo == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<Book>(bookFromRepo));
        }

        

       ///// <summary>
       ///// Get the book by Book Id and Author Id.
       ///// </summary>
       ///// <param name="authorId">The Author Id.</param>
       ///// <param name="bookId">The book Id</param>
       ///// <returns>Ac action result type of book with author first name and last name concatenated as one Author name.</returns>
       //[ProducesResponseType(StatusCodes.Status404NotFound)]
       //[ProducesResponseType(StatusCodes.Status400BadRequest)]
       //[ProducesResponseType(StatusCodes.Status200OK)]
       //[Produces("application/json","application/xml")]
       //[RequestHeaderMatchesMediaType(HeaderNames.Accept,"application/json","application/vnd.marvin.bookwithconcatenatedname+json")]
       //[HttpGet("{bookId}")]
       //public async Task<ActionResult<BookWithConcatenatedAuthorName>> GetBookWithConcatenatedname(
       //    Guid authorId,
       //    Guid bookId)
       //{
       //    if (! await _authorRepository.AuthorExistsAsync(authorId))
       //    {
       //        return NotFound();
       //    }

       //    var bookFromRepo = await _bookRepository.GetBookAsync(authorId, bookId);
       //    if (bookFromRepo == null)
       //    {
       //        return NotFound();
       //    }

       //    return Ok(_mapper.Map<Book>(bookFromRepo));
       //}

        [HttpPost()]
        public async Task<ActionResult<Book>> CreateBook(
            Guid authorId,
            [FromBody] BookForCreation bookForCreation)
        {
            if (!await _authorRepository.AuthorExistsAsync(authorId))
            {
                return NotFound();
            }

            var bookToAdd = _mapper.Map<Entities.Book>(bookForCreation);
            _bookRepository.AddBook(bookToAdd);
            await _bookRepository.SaveChangesAsync();

            return CreatedAtRoute(
                "GetBook",
                new { authorId, bookId = bookToAdd.Id },
                _mapper.Map<Book>(bookToAdd));
        }
    }
}
