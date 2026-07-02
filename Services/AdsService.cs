using Dapper;
using FidsSystem.Models;

namespace FidsSystem.Services
{
    public class AdsService : BaseRepository, IAdsService
    {
        public AdsService(IConfiguration configuration) : base(configuration) { }

        public async Task<IEnumerable<Advertisement>> GetAllAdsAsync(string? search = null, int? month = null, int? year = null)
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<Advertisement>("sp_Ad_GetAll",
                new { Search = search, Month = month, Year = year },
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<Advertisement?> GetAdByIdAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<Advertisement>("sp_Ad_GetById",
                new { Id = id },
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<int> CreateAdAsync(Advertisement ad)
        {
            using var conn = CreateConnection();
            var result = await conn.QueryFirstOrDefaultAsync<dynamic>("sp_Ad_Create",
                new { ad.Title, ad.FileType, ad.FilePath, ad.Duration, ad.FileSizeMB, ad.Month, ad.Year },
                commandType: System.Data.CommandType.StoredProcedure);
            return (int)(result?.NewId ?? 0);
        }

        public async Task DeleteAdAsync(int id)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync("sp_Ad_Delete",
                new { Id = id },
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<AdPlaylist>> GetAllPlaylistsAsync(string? search = null)
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<AdPlaylist>("sp_Playlist_GetAll",
                new { Search = search },
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<AdPlaylist?> GetPlaylistByIdAsync(int id)
        {
            using var conn = CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<AdPlaylist>("sp_Playlist_GetById",
                new { Id = id },
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<int> CreatePlaylistAsync(AdPlaylist playlist)
        {
            using var conn = CreateConnection();
            var result = await conn.QueryFirstOrDefaultAsync<dynamic>("sp_Playlist_Create",
                new { playlist.Name },
                commandType: System.Data.CommandType.StoredProcedure);
            return (int)(result?.NewId ?? 0);
        }

        public async Task UpdatePlaylistAsync(AdPlaylist playlist)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync("sp_Playlist_Update",
                new { playlist.Id, playlist.Name },
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task DeletePlaylistAsync(int id)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync("sp_Playlist_Delete",
                new { Id = id },
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<MatchDisplay>> GetAllMatchesAsync(string? search = null, int? zoneId = null, string? status = null)
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<MatchDisplay>("sp_Match_GetAllByDisplay",
                new { Search = search, ZoneId = zoneId, Status = status },
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task SetMatchByDisplayAsync(int displayId, int playlistId)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync("sp_Match_SetByDisplay",
                new { DisplayId = displayId, PlaylistId = playlistId },
                commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task RemoveMatchByDisplayAsync(int displayId)
        {
            using var conn = CreateConnection();
            await conn.ExecuteAsync("sp_Match_RemoveByDisplay",
                new { DisplayId = displayId },
                commandType: System.Data.CommandType.StoredProcedure);
        }
    }
}
