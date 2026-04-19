using FidsSystem.Models;

namespace FidsSystem.Services
{
    public interface IAdsService
    {
        // Advertisements
        Task<IEnumerable<Advertisement>> GetAllAdsAsync(string? search = null, int? month = null, int? year = null);
        Task<Advertisement?> GetAdByIdAsync(int id);
        Task<int> CreateAdAsync(Advertisement ad);
        Task DeleteAdAsync(int id);

        // Playlists
        Task<IEnumerable<AdPlaylist>> GetAllPlaylistsAsync(string? search = null);
        Task<AdPlaylist?> GetPlaylistByIdAsync(int id);
        Task<int> CreatePlaylistAsync(AdPlaylist playlist);
        Task UpdatePlaylistAsync(AdPlaylist playlist);
        Task DeletePlaylistAsync(int id);

        // Match Displays (device-centric)
        Task<IEnumerable<MatchDisplay>> GetAllMatchesAsync(string? search = null, int? zoneId = null, string? status = null);
        Task SetMatchByDisplayAsync(int displayId, int playlistId);
        Task RemoveMatchByDisplayAsync(int displayId);
    }
}
