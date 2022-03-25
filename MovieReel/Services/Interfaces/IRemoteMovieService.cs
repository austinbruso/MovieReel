using MovieReel.Enums;
using MovieReel.Models.TMDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MovieReel.Services.Interfaces
{
    public interface IRemoteMovieService
    {
        Task<MovieDetails> MovieDetailAsync(int id);
        Task<MovieSearch> SearchMoviesAsync(MovieCategory category, int count);
        Task<ActorDetails> ActorDetailAsync(int id);

    }
}
