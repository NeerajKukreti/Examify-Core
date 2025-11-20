using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using DataModel;
using Model.DTO;

namespace DAL.Repository
{
    public interface IInstituteRepository
    {
        Task<IEnumerable<InstituteModel>> GetAllInstitutesAsync();
        Task<InstituteModel?> GetInstituteByIdAsync(int instituteId);
        Task<int> InsertOrUpdateInstituteAsync(InstituteDTO dto, int? instituteId = null, int? createdBy = null, int? modifiedBy = null);
    }

    public class InstituteRepository : IInstituteRepository
    {
        private readonly IConfiguration _config;
        public InstituteRepository(IConfiguration config) => _config = config;

        private IDbConnection CreateConnection() => new SqlConnection(_config.GetConnectionString("DefaultConnection"));

        public async Task<IEnumerable<InstituteModel>> GetAllInstitutesAsync()
        {
            using var connection = CreateConnection();
            return await connection.QueryAsync<InstituteModel>(
                "_sp_GetAllInstitutes",
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<InstituteModel?> GetInstituteByIdAsync(int instituteId)
        {
            using var connection = CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<InstituteModel>(
                "_sp_GetInstituteById",
                new { InstituteId = instituteId },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<int> InsertOrUpdateInstituteAsync(InstituteDTO dto, int? instituteId = null, int? createdBy = null, int? modifiedBy = null)
        {
            using var connection = CreateConnection();
            var parameters = new DynamicParameters();

            parameters.Add("@InstituteId", instituteId);
            parameters.Add("@InstituteName", dto.InstituteName);
            parameters.Add("@ShortName", dto.ShortName);
            parameters.Add("@Logo", dto.Logo);
            parameters.Add("@Address", dto.Address);
            parameters.Add("@City", dto.City);
            parameters.Add("@StateId", dto.StateId);
            parameters.Add("@Pincode", dto.Pincode);
            parameters.Add("@Phone", dto.Phone);
            parameters.Add("@Email", dto.Email);
            parameters.Add("@PrimaryContact", dto.PrimaryContact);
            parameters.Add("@SecondaryContact", dto.SecondaryContact);
            parameters.Add("@ActivationDate", dto.ActivationDate);
            parameters.Add("@Validity", dto.Validity);
            parameters.Add("@IsActive", dto.IsActive);
            parameters.Add("@CreatedBy", createdBy);
            parameters.Add("@ModifiedBy", modifiedBy);
            parameters.Add("@UserId", dto.UserId);

            var newInstituteId = await connection.ExecuteScalarAsync<int>(
                "_sp_InsertUpdateInstitute",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return newInstituteId;
        }

    }
}
