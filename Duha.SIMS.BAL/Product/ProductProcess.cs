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
    public class ProductProcess : SIMSBalOdataBase<ProductSM>
    {
        #region Properties
        private readonly ILoginUserDetail _loginUserDetail;
        private readonly ApiDbContext _apiDbContext; 
        #endregion Properties

        #region Constructor
        public ProductProcess(IMapper mapper, ILoginUserDetail loginUserDetail, ApiDbContext apiDbContext)
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
        /// Return IQueryable ProductSM
        /// </returns>
        public override async Task<IQueryable<ProductSM>> GetServiceModelEntitiesForOdata()
        {
            var entitySet = _apiDbContext.Products;
            var query = entitySet.Select(entity => new ProductSM
            {
                Name = entity.Name,
                Image = ConvertImageToBase64(entity.Image),
                CategoryId = entity.CategoryId,
                BrandId = entity.BrandId,
                UnitId = entity.UnitId,
                Variant = entity.Variant,
                Code = entity.Code,
                Price = entity.Price,
                Quantity = entity.Quantity,
                Status = entity.Status,
                Id = entity.Id,
                CreatedBy = entity.CreatedBy,
                LastModifiedBy = entity.LastModifiedBy,
                CreatedOnUTC = entity.CreatedOnUTC,
                LastModifiedOnUTC = entity.LastModifiedOnUTC,
            });

            // Return the projected query as IQueryable
            return await Task.FromResult(query);
        }

        static string ConvertImageToBase64(string Image)
        {
            if (Image != null)
            {


                // Read the image file and convert it to base64 string
                byte[] imageBytes = File.ReadAllBytes(Image);
                string base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }
            return null;
        }

        #endregion Odata

        #region Get All
        /// <summary>
        /// Fetches all the Products from the database, with images in base 64 format
        /// </summary>
        /// <returns>
        /// If Successful, Returns List of ProductSM otherwise return null
        /// </returns>
        public async Task<List<ProductSM>> GetAllProducts(int skip, int top)
        {
            var itemsFromDb = await _apiDbContext.Products
                .Skip(skip).Take(top)
                .ToListAsync();
            var response = new List<ProductSM>();
            // Return an empty list instead of null if there are no items
            if (itemsFromDb == null || itemsFromDb.Count == 0)
            {
                return null;
            }
            foreach (var item in itemsFromDb) 
            { 
                var sm = await GetProductsById(item.Id);
                response.Add(sm);
            }
            return response;
        }

        public async Task<int> GetAllProductsCount()
        {
            var count =  _apiDbContext.Products.AsNoTracking().Count();
            return count;
        }

        #endregion Get All

        #region Get By Id
        /// <summary>
        /// Get Product By Using ProductId
        /// </summary>
        /// <param name="Id">Using Id of Product Fetches the respective Product</param>
        /// <returns> 
        /// If Successful, returns ProductSM otherwise returns Null.
        /// </returns>
        /// <exception cref="ShopWaveException"></exception>
        public async Task<ProductSM?> GetProductsById(int Id)
        {
            var singleItemFromDb = await _apiDbContext.Products.FindAsync(Id);

            if (singleItemFromDb == null)
            {
                return null;
            }

            string base64Image = null;

            // Only convert to base64 if Image is not null or empty
            if (!string.IsNullOrEmpty(singleItemFromDb.Image))
            {
                base64Image = await ConvertToBase64(singleItemFromDb.Image);
                singleItemFromDb.Image = base64Image;
            }

            return _mapper.Map<ProductSM>(singleItemFromDb);
        }



        #endregion Get By Id

        #region Add
        /// <summary>
        /// Adds a new Product to the database.
        /// </summary>
        /// <param name="ProductSM">The ProductSM object representing the new Product.</param>
        /// <returns>
        /// If successful, returns the added ProductSM; otherwise, returns null.
        /// </returns>
        public async Task<ProductSM?> AddProduct(ProductSM objSM)
        {
            string? ProductImageRelativePath = null;
            if (objSM == null)
                return null;
            var dm = _mapper.Map<ProductDM>(objSM);
            dm.CreatedBy = _loginUserDetail.LoginId;
            dm.CreatedOnUTC = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(objSM.Image))
            {
                ProductImageRelativePath = await SaveFromBase64(objSM.Image);
            }

            dm.Image = ProductImageRelativePath;
            await _apiDbContext.Products.AddAsync(dm);
            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                return _mapper.Map<ProductSM>(dm);
            }
            if (ProductImageRelativePath != null)
            {
                string fullImage = Path.GetFullPath(ProductImageRelativePath);
                if (File.Exists(fullImage))
                    File.Delete(fullImage);
            }
            throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, "Something went wrong while saving the changes");
            
        }
        #endregion Add

        #region Update
        /// <summary>
        /// Updates a Product in the database.
        /// </summary>
        /// <param name="objIdToUpdate">The Id of the Product to update.</param>
        /// <param name="ProductSM">The updated ProductSM object.</param>
        /// <returns>
        /// If successful, returns the updated ProductSM; otherwise, returns null.
        /// </returns>
        public async Task<ProductSM?> UpdateProducts(int objIdToUpdate, ProductSM objSM)
        {
            if (objSM != null && objIdToUpdate > 0)
            {
                //retrieve target product category to update from db
                ProductDM? objDM = await _apiDbContext.Products.FindAsync(objIdToUpdate);

                if (objDM != null)
                {
                    // Get product category image full path
                    var imageFullPath = Path.GetFullPath(objDM.Image);

                    objSM.Id = objIdToUpdate;
                    _mapper.Map(objSM, objDM);

                    //converts base 64 string to image and stores it inside a folder and returns relative path of the image
                    var imageRelativePath = await SaveFromBase64(objSM.Image);

                    if (imageRelativePath != null)
                    {
                        objDM.Image = imageRelativePath;
                        objDM.LastModifiedBy = _loginUserDetail.LoginId;
                        objDM.LastModifiedOnUTC = DateTime.UtcNow;

                        if (await _apiDbContext.SaveChangesAsync() > 0)
                        {
                            // Delete the previous image from the folder
                            if (File.Exists(imageFullPath))
                                File.Delete(imageFullPath);

                            return _mapper.Map<ProductSM>(objDM);
                        }
                    }
                    return null;
                }
                else
                {
                    throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, "Product to update not found, add as new instead.");
                }
            }
            return null;
        }

        #endregion Update

        #region Delete
        /// <summary>
        /// Deletes a Product by its Id from the database.
        /// </summary>
        /// <param name="id">The Id of the Product to be deleted.</param>
        /// <returns>
        /// A DeleteResponseRoot indicating whether the deletion was successful or not.
        /// </returns>
        public async Task<DeleteResponseRoot> DeleteProductsById(int id)
        {
            // Check if a Product with the specified Id is present in the database
            var itemToDelete = await _apiDbContext.Products.FindAsync(id);

            if (itemToDelete != null)
            {
                //retrieve Product image relativePath to delete it from the folder as well
                var Image = itemToDelete.Image;

                _apiDbContext.Products.Remove(itemToDelete);

                // If save changes is successful, return a success response
                if (await _apiDbContext.SaveChangesAsync() > 0)
                {
                    //get full resume path from relative resume path
                    if (File.Exists(Path.GetFullPath(Image)))
                        File.Delete(Image);

                    return new DeleteResponseRoot(true, $"Product with Id {id} deleted successfully!");
                }
            }
            // If no Product with the specified Id is found, return a failure response
            return new DeleteResponseRoot(false, "Product not found");
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
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\content\Products");

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
