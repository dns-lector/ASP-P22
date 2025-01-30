namespace ASP_P22.Services.Storage
{
    public interface IStorageService
    {
        String Save(IFormFile formFile);

        Stream? Get(String fileUrl);

        Boolean Delete(String fileUrl);
    }
}
