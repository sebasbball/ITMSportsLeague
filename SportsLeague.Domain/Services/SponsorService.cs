using Microsoft.Extensions.Logging;
using SportsLeague.Domain.Entities;
using SportsLeague.Domain.Interfaces.Repositories;
using SportsLeague.Domain.Interfaces.Services;

namespace SportsLeague.Domain.Services
{
    public class SponsorService : ISponsorService
    {
        private readonly ISponsorRepository _sponsorRepository;
        private readonly ITournamentRepository _tournamentRepository;
        private readonly ITournamentSponsorRepository _tournamentSponsorRepository;
        private readonly ILogger<SponsorService> _logger;

        public SponsorService(
            ISponsorRepository sponsorRepository,
            ITournamentRepository tournamentRepository,
            ITournamentSponsorRepository tournamentSponsorRepository,
            ILogger<SponsorService> logger)
        {
            _sponsorRepository = sponsorRepository;
            _tournamentRepository = tournamentRepository;
            _tournamentSponsorRepository = tournamentSponsorRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Sponsor>> GetAllAsync()
        {
            _logger.LogInformation("Retrieving all sponsors");
            return await _sponsorRepository.GetAllAsync();
        }

        public async Task<Sponsor?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Retrieving sponsor with ID: {SponsorId}", id);
            var sponsor = await _sponsorRepository.GetByIdAsync(id);

            if (sponsor == null)
                _logger.LogWarning("Sponsor with ID {SponsorId} not found", id);

            return sponsor;
        }

        public async Task<Sponsor> CreateAsync(Sponsor sponsor)
        {
            // Validar nombre único
            var existing = await _sponsorRepository.GetByNameAsync(sponsor.Name);
            if (existing != null)
            {
                _logger.LogWarning("Sponsor with name '{SponsorName}' already exists", sponsor.Name);
                throw new InvalidOperationException(
                    $"Ya existe un sponsor con el nombre '{sponsor.Name}'");
            }

            // Validar formato de email
            if (!sponsor.ContactEmail.Contains('@'))
            {
                throw new InvalidOperationException(
                    "El email de contacto no tiene un formato válido");
            }

            _logger.LogInformation("Creating sponsor: {SponsorName}", sponsor.Name);
            return await _sponsorRepository.CreateAsync(sponsor);
        }

        public async Task UpdateAsync(int id, Sponsor sponsor)
        {
            var existing = await _sponsorRepository.GetByIdAsync(id);
            if (existing == null)
                throw new KeyNotFoundException($"No se encontró el sponsor con ID {id}");

            // Validar nombre único si cambió
            if (existing.Name != sponsor.Name)
            {
                var sponsorWithSameName = await _sponsorRepository.GetByNameAsync(sponsor.Name);
                if (sponsorWithSameName != null)
                    throw new InvalidOperationException(
                        $"Ya existe un sponsor con el nombre '{sponsor.Name}'");
            }

            // Validar formato de email
            if (!sponsor.ContactEmail.Contains('@'))
                throw new InvalidOperationException(
                    "El email de contacto no tiene un formato válido");

            existing.Name = sponsor.Name;
            existing.ContactEmail = sponsor.ContactEmail;
            existing.Phone = sponsor.Phone;
            existing.WebsiteUrl = sponsor.WebsiteUrl;
            existing.Category = sponsor.Category;

            _logger.LogInformation("Updating sponsor with ID: {SponsorId}", id);
            await _sponsorRepository.UpdateAsync(existing);
        }

        public async Task DeleteAsync(int id)
        {
            var exists = await _sponsorRepository.ExistsAsync(id);
            if (!exists)
                throw new KeyNotFoundException($"No se encontró el sponsor con ID {id}");

            _logger.LogInformation("Deleting sponsor with ID: {SponsorId}", id);
            await _sponsorRepository.DeleteAsync(id);
        }

        public async Task LinkToTournamentAsync(int sponsorId, int tournamentId, decimal contractAmount)
        {
            // Validar que el sponsor existe
            var sponsorExists = await _sponsorRepository.ExistsAsync(sponsorId);
            if (!sponsorExists)
                throw new KeyNotFoundException($"No se encontró el sponsor con ID {sponsorId}");

            // Validar que el torneo existe
            var tournament = await _tournamentRepository.GetByIdAsync(tournamentId);
            if (tournament == null)
                throw new KeyNotFoundException($"No se encontró el torneo con ID {tournamentId}");

            // Validar que no esté ya vinculado
            var existing = await _tournamentSponsorRepository
                .GetByTournamentAndSponsorAsync(tournamentId, sponsorId);
            if (existing != null)
                throw new InvalidOperationException(
                    "Este sponsor ya está vinculado a este torneo");

            // Validar que el monto sea mayor a 0
            if (contractAmount <= 0)
                throw new InvalidOperationException(
                    "El monto del contrato debe ser mayor a 0");

            var tournamentSponsor = new TournamentSponsor
            {
                SponsorId = sponsorId,
                TournamentId = tournamentId,
                ContractAmount = contractAmount,
                JoinedAt = DateTime.UtcNow
            };

            _logger.LogInformation(
                "Linking sponsor {SponsorId} to tournament {TournamentId}",
                sponsorId, tournamentId);
            await _tournamentSponsorRepository.CreateAsync(tournamentSponsor);
        }

        public async Task UnlinkFromTournamentAsync(int sponsorId, int tournamentId)
        {
            // Validar que la relación existe
            var existing = await _tournamentSponsorRepository
                .GetByTournamentAndSponsorAsync(tournamentId, sponsorId);
            if (existing == null)
                throw new KeyNotFoundException(
                    "Este sponsor no está vinculado a este torneo");

            _logger.LogInformation(
                "Unlinking sponsor {SponsorId} from tournament {TournamentId}",
                sponsorId, tournamentId);
            await _tournamentSponsorRepository.DeleteAsync(existing.Id);
        }

        public async Task<IEnumerable<TournamentSponsor>> GetTournamentsBySponsorAsync(int sponsorId)
        {
            var exists = await _sponsorRepository.ExistsAsync(sponsorId);
            if (!exists)
                throw new KeyNotFoundException($"No se encontró el sponsor con ID {sponsorId}");

            _logger.LogInformation("Retrieving tournaments for sponsor ID: {SponsorId}", sponsorId);
            return await _tournamentSponsorRepository.GetByTournamentAsync(sponsorId);
        }
    }
}