using Microsoft.AspNetCore.Mvc;
using MovieReel.Data;
using MovieReel.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MovieReel.Models.TMDB;

namespace MovieReel.Controllers
{
    public class ActorsController : Controller
    {

        private readonly IRemoteMovieService _tmbdMovieService;
        private readonly IDataMappingService _mappingService;

        public ActorsController(IRemoteMovieService tmbdMovieService, IDataMappingService mappingService)
        {
            _tmbdMovieService = tmbdMovieService;
            _mappingService = mappingService;
        }

        
        public async Task< IActionResult> Details(int id)
        {
            var actor = await _tmbdMovieService.ActorDetailAsync(id);
            actor = _mappingService.MapActorDetail(actor);

            return View(actor);
        }
    }
}
