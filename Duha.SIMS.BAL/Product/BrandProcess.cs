using AutoMapper;
using Duha.SIMS.BAL.Base;
using Duha.SIMS.BAL.Exceptions;
using Duha.SIMS.DAL.Contexts;
using Duha.SIMS.DomainModels.Product;
using Duha.SIMS.ServiceModels.CommonResponse;
using Duha.SIMS.ServiceModels.Enums;
using Duha.SIMS.ServiceModels.LoggedInIdentity;
using Duha.SIMS.ServiceModels.Product;
using Microsoft.EntityFrameworkCore;

namespace Duha.SIMS.BAL.Product
{
    public class BrandProcess : SIMSBalOdataBase<BrandSM>
    {
        #region Properties
        private readonly ILoginUserDetail _loginUserDetail;
        private readonly ApiDbContext _apiDbContext; 
        #endregion Properties

        #region Constructor
        public BrandProcess(IMapper mapper, ILoginUserDetail loginUserDetail, ApiDbContext apiDbContext)
            : base(mapper, apiDbContext)
        {
            _loginUserDetail = loginUserDetail;
            _apiDbContext = apiDbContext;
        }
        #endregion Constructor

        #region Odata
        /// <summary>
        /// Gets Service Model Entities For OData
        /// </summary>
        /// <returns>
        /// Return IQueryable BrandSM
        /// </returns>
        public override async Task<IQueryable<BrandSM>> GetServiceModelEntitiesForOdata()
        {
            var entitySet = _apiDbContext.Brands;
            var query = entitySet.Select(entity => new BrandSM
            {
                Name = entity.Name,
                ImagePath = ConvertImagePathToBase64(entity.ImagePath),
                Id = entity.Id,
                CreatedBy = entity.CreatedBy,
                LastModifiedBy = entity.LastModifiedBy,
                CreatedOnUTC = entity.CreatedOnUTC,
                LastModifiedOnUTC = entity.LastModifiedOnUTC,
            });

            // Return the projected query as IQueryable
            return await Task.FromResult(query);
        }

        static string ConvertImagePathToBase64(string imagePath)
        {
            if (imagePath != null)
            {


                // Read the image file and convert it to base64 string
                byte[] imageBytes = File.ReadAllBytes(imagePath);
                string base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }
            return null;
        }

        #endregion Odata

        #region Get All
        /// <summary>
        /// Fetches all the brands from the database, with images in base 64 format
        /// </summary>
        /// <returns>
        /// If Successful, Returns List of BrandSM otherwise return null
        /// </returns>
        public async Task<List<BrandSM>> GetAllBrands()
        {
            var itemsFromDb = await _apiDbContext.Brands.ToListAsync();

            // Return an empty list instead of null if there are no items
            if (itemsFromDb == null || itemsFromDb.Count == 0)
            {
                return null;
            }

            return _mapper.Map<List<BrandSM>>(itemsFromDb);
        }

        #endregion Get All

        #region Get By Id
        /// <summary>
        /// Get Brand By Using BrandId
        /// </summary>
        /// <param name="Id">Using Id of Brand Fetches the respective Brand</param>
        /// <returns> 
        /// If Successful, returns BrandSM otherwise returns Null.
        /// </returns>
        /// <exception cref="ShopWaveException"></exception>
        public async Task<BrandSM?> GetbrandsById(int Id)
        {
            var singleItemFromDb = await _apiDbContext.Brands.FindAsync(Id);

            if (singleItemFromDb == null)
            {
                return null;
            }

            string base64Image = null;

            // Only convert to base64 if ImagePath is not null or empty
            if (!string.IsNullOrEmpty(singleItemFromDb.ImagePath))
            {
                base64Image = await ConvertToBase64(singleItemFromDb.ImagePath);
                singleItemFromDb.ImagePath = base64Image;
            }

            return _mapper.Map<BrandSM>(singleItemFromDb);
        }



        #endregion Get By Id

        #region Add
        /// <summary>
        /// Adds a new brand to the database.
        /// </summary>
        /// <param name="BrandSM">The BrandSM object representing the new brand.</param>
        /// <returns>
        /// If successful, returns the added BrandSM; otherwise, returns null.
        /// </returns>
        public async Task<BrandSM?> Addbrands(BrandSM objSM)
        {
            string? brandImageRelativePath = null;
            if (objSM == null)
                return null;
            var dm = _mapper.Map<BrandDM>(objSM);
            dm.CreatedBy = _loginUserDetail.LoginId;
            dm.CreatedOnUTC = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(objSM.ImagePath))
            {
                brandImageRelativePath = await SaveFromBase64(objSM.ImagePath);
            }

            dm.ImagePath = brandImageRelativePath;
            await _apiDbContext.Brands.AddAsync(dm);
            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                return _mapper.Map<BrandSM>(dm);
            }
            if (brandImageRelativePath != null)
            {
                string fullImagePath = Path.GetFullPath(brandImageRelativePath);
                if (File.Exists(fullImagePath))
                    File.Delete(fullImagePath);
            }
            throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, "Something went wrong while saving the changes");
            
        }
        #endregion Add

        #region Update
        /// <summary>
        /// Updates a brand in the database.
        /// </summary>
        /// <param name="objIdToUpdate">The Id of the brand to update.</param>
        /// <param name="BrandSM">The updated BrandSM object.</param>
        /// <returns>
        /// If successful, returns the updated BrandSM; otherwise, returns null.
        /// </returns>
        public async Task<BrandSM?> UpdateBrands(int objIdToUpdate, BrandSM BrandSM)
        {
            if (BrandSM != null && objIdToUpdate > 0)
            {
                //retrieve target product category to update from db
                BrandDM? objDM = await _apiDbContext.Brands.FindAsync(objIdToUpdate);

                if (objDM != null)
                {
                    // Get product category image full path
                    var imageFullPath = Path.GetFullPath(objDM.ImagePath);

                    BrandSM.Id = objIdToUpdate;
                    _mapper.Map(BrandSM, objDM);

                    //converts base 64 string to image and stores it inside a folder and returns relative path of the image
                    var imageRelativePath = await SaveFromBase64(BrandSM.ImagePath);

                    if (imageRelativePath != null)
                    {
                        objDM.ImagePath = imageRelativePath;
                        objDM.LastModifiedBy = _loginUserDetail.LoginId;
                        objDM.LastModifiedOnUTC = DateTime.UtcNow;

                        if (await _apiDbContext.SaveChangesAsync() > 0)
                        {
                            // Delete the previous image from the folder
                            if (File.Exists(imageFullPath))
                                File.Delete(imageFullPath);

                            return _mapper.Map<BrandSM>(objDM);
                        }
                    }
                    return null;
                }
                else
                {
                    throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, "Brand to update not found, add as new instead.");
                }
            }
            return null;
        }

        #endregion Update

        #region Delete
        /// <summary>
        /// Deletes a brand by its Id from the database.
        /// </summary>
        /// <param name="id">The Id of the brand to be deleted.</param>
        /// <returns>
        /// A DeleteResponseRoot indicating whether the deletion was successful or not.
        /// </returns>
        public async Task<DeleteResponseRoot> DeleteBrandsById(int id)
        {
            // Check if a brand with the specified Id is present in the database
            var itemToDelete = await _apiDbContext.Brands.FindAsync(id);

            if (itemToDelete != null)
            {
                //retrieve brand image relativePath to delete it from the folder as well
                var imagePath = itemToDelete.ImagePath;

                _apiDbContext.Brands.Remove(itemToDelete);

                // If save changes is successful, return a success response
                if (await _apiDbContext.SaveChangesAsync() > 0)
                {
                    //get full resume path from relative resume path
                    if (File.Exists(Path.GetFullPath(imagePath)))
                        File.Delete(imagePath);

                    return new DeleteResponseRoot(true, $"Brand with Id {id} deleted successfully!");
                }
            }
            // If no brand with the specified Id is found, return a failure response
            return new DeleteResponseRoot(false, "Brand not found");
        }
        #endregion Delete

        #region Private Functions
        /// <summary>
        /// Saves a base64 encoded string as a jpg/jpeg/png etc file on the server.
        /// </summary>
        /// <param name="base64String">The base64 encoded string of the jpg/jpeg/png etc.</param>
        /// <returns>
        /// If successful, returns the relative file path of the saved file; 
        /// otherwise, returns null.
        /// </returns>
        static async Task<string?> SaveFromBase64(string base64String)
        {
            string? filePath = null;
            string imageExtension = "jpg";
            try
            {
                //convert bas64string to bytes
                byte[] imageBytes = Convert.FromBase64String(base64String);

                // Check if the file size exceeds 1MB (2 * 1024 * 1024 bytes)
                if (imageBytes.Length > 1 * 1024 * 1024) //change 1 to desired size 2,3,4 etc
                {
                    throw new Exception("File size exceeds 1 Mb limit.");
                }

                string fileName = Guid.NewGuid().ToString() + "." + imageExtension;

                // Specify the folder path where resumes are stored
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\content\brands");

                // Create the folder if it doesn't exist
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // Combine the folder path and file name to get the full file path
                filePath = Path.Combine(folderPath, fileName);

                // Write the bytes to the file asynchronously
                await File.WriteAllBytesAsync(filePath, imageBytes);

                // Return the relative file path
                return Path.GetRelativePath(Directory.GetCurrentDirectory(), filePath);
            }
            catch
            {
                // If an error occurs, delete the file (if created) and return null
                if (File.Exists(filePath))
                    File.Delete(filePath);
                throw;
            }
        }


        /// <summary>
        /// Converts an image file to a base64 encoded string.
        /// </summary>
        /// <param name="filePath">The path to the image file.</param>
        /// <returns>
        /// If successful, returns the base64 encoded string; 
        /// otherwise, returns null.
        /// </returns>
        static async Task<string?> ConvertToBase64(string filePath)
        {
            try
            {
                // Read all bytes from the file asynchronously
                byte[] imageBytes = await File.ReadAllBytesAsync(filePath);

                // Convert the bytes to a base64 string
                return Convert.ToBase64String(imageBytes);
            }
            catch (Exception ex)
            {
                // Handle exceptions and return null
                return ex.Message;
            }
        }
        #endregion Private Functions
    }
}
