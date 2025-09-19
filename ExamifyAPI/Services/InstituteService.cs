using DAL.Repository;
using DataModel;
using ExamAPI.Services;
using Model.DTO;

namespace ExamifyAPI.Services
{
    public interface IInstituteService
    {
        Task<IEnumerable<InstituteModel>> GetAllInstitutesAsync();
        Task<InstituteModel?> GetInstituteByIdAsync(int instituteId);
        Task<int> InsertOrUpdateInstituteAsync(
            InstituteDTO dto,
            int? instituteId = null,
            int? createdBy = null,
            int? modifiedBy = null
        );
    }

    public class InstituteService : IInstituteService
    {
        private readonly IInstituteRepository _instituteRepository;
        private readonly IAuthService _authService;

        public InstituteService(IInstituteRepository instituteRepository, IAuthService authService)
        {
            _instituteRepository = instituteRepository;
            _authService = authService;
        }

        public async Task<IEnumerable<InstituteModel>> GetAllInstitutesAsync()
        {
            return await _instituteRepository.GetAllInstitutesAsync();
        }

        public async Task<InstituteModel?> GetInstituteByIdAsync(int instituteId)
        {
            return await _instituteRepository.GetInstituteByIdAsync(instituteId);
        }

        public async Task<int> InsertOrUpdateInstituteAsync(InstituteDTO dto, int? instituteId = null, int? createdBy = null, int? modifiedBy = null)
        {
            // If inserting new institute, create user first
            if (instituteId == null)
            {
                var userId = await _authService.Register(dto.PrimaryContact, dto.Password, "Institute");
                dto.UserId = userId; // assign to DTO so repository gets the correct UserId
            }

            // For updates, dto.UserId is already coming from DB, so no need to overwrite
            return await _instituteRepository.InsertOrUpdateInstituteAsync(
                dto,
                instituteId,
                createdBy,
                modifiedBy
            );
        }
    }
}
