﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MovieProDemo.Services;
using MovieReel.Data;
using MovieReel.Models.Database;
using MovieReel.Models.Settings;
using MovieReel.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MovieReel.Controllers
{
    public class MoviesController : Controller
    {

        private readonly AppSettings _appSettings;
        private readonly ApplicationDbContext _context;
        private readonly IImageService _imageService;
        private readonly IRemoteMovieService _tmbdMovieService;
        private readonly IDataMappingService _tmdbMappingService;

        public MoviesController(IOptions<AppSettings> appSettings, ApplicationDbContext context, IImageService imageService, IRemoteMovieService tmbdMovieService, IDataMappingService tmdbMappingService)
        {
            _appSettings = appSettings.Value;
            _context = context;
            _imageService = imageService;
            _tmbdMovieService = tmbdMovieService;
            _tmdbMappingService = tmdbMappingService;
        }

        public async Task <IActionResult> Import()
        {
            var movies = await _context.Movie.ToListAsync();
            return View(movies);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task <IActionResult> Import(int id)
        {
            // If movie already exists sends a warning alert
            if (_context.Movie.Any(m => m.MovieId == id))
            {
                var localMovie = await _context.Movie.FirstOrDefaultAsync(m => m.MovieId == id);
                return RedirectToAction("Details", "Movies", new { id = localMovie.Id, local = true });
            }

            // Step 1: Get the raw data from the API
            var movieDetail = await _tmbdMovieService.MovieDetailAsync(id);

            // Step 2: Run the data through a mapping procedure 
            var movie = await _tmdbMappingService.MapMovieDetailAsync(movieDetail);

            //Step 3: Add the new movie
            _context.Add(movie);
            await _context.SaveChangesAsync();

            //Step 4: Assign it to the default all collection
            await AddToMovieCollection(movie.Id, _appSettings.MovieReelSettings.DefaultCollection.Name);

            return RedirectToAction("Import");

        }

        public async Task<IActionResult> Library()
        {
            var movies = await _context.Movie.ToListAsync();
            return View(movies);
        }



        // GET: Temp/Create
        public IActionResult Create()
        {

            ViewData["CollectionId"] = new SelectList(_context.Collection, "Id", "Name");
            return View();
        }

        // POST: Temp/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,MovieId,Title,TagLine,Overview,RunTime,ReleaseDate,Rating,VoteAverage,Poster,PosterType,Backdrop,BackdropType,TrailerUrl")] Movie movie, int collectionId)
        {
            if (ModelState.IsValid)
            {
                movie.PosterType = movie.PosterFile?.ContentType;
                movie.Poster = await _imageService.EncodeImageAsync(movie.PosterFile);

                movie.BackdropType = movie.BackdropFile?.ContentType;
                movie.Backdrop = await _imageService.EncodeImageAsync(movie.BackdropFile);

                _context.Add(movie);
                await _context.SaveChangesAsync();


                await AddToMovieCollection(movie.Id, collectionId);

                return RedirectToAction("Index", "MovieCollections");
            }
            return View(movie);
        }

        // GET: Temp/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }

        // POST: Temp/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MovieId,Title,TagLine,Overview,RunTime,ReleaseDate,Rating,VoteAverage,Poster,PosterType,Backdrop,BackdropType,TrailerUrl")] Movie movie)
        {
            if (id != movie.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        // GET: Temp/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Temp/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movie.FindAsync(id);
            _context.Movie.Remove(movie);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movie.Any(e => e.Id == id);
        }


        private async Task AddToMovieCollection(int movieId, string collectionName)
        {
            var collection = await _context.Collection.FirstOrDefaultAsync(c => c.Name == collectionName);
            _context.Add(
                new MovieCollection()
                {
                    CollectionId = collection.Id,
                    MovieId = movieId
                }
                );
            await _context.SaveChangesAsync();


        }

        private async Task AddToMovieCollection(int movieId, int collectionId)
        {
            
            _context.Add(
                new MovieCollection()
                {
                    CollectionId = collectionId,
                    MovieId = movieId
                }
                );
            await _context.SaveChangesAsync();


        }

    }
}