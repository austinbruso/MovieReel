using MovieReel.Models.Database;
using MovieReel.Models.TMDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MovieReel.Services.Interfaces
{
    public interface IDataMappingService
    {
        Task<Movie> MapMovieDetailAsync(MovieDetails movies);
        ActorDetails MapActorDetails(ActorDetails actor);

    }
}
