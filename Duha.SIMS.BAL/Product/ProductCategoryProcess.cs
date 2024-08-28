using AutoMapper;
using Duha.SIMS.BAL.Base;
using Duha.SIMS.BAL.Exceptions;
using Duha.SIMS.DAL.Contexts;
using Duha.SIMS.DomainModels.Enums;
using Duha.SIMS.DomainModels.Product;
using Duha.SIMS.ServiceModels.Enums;
using Duha.SIMS.ServiceModels.LoggedInIdentity;
using Duha.SIMS.ServiceModels.Product;
using Microsoft.EntityFrameworkCore;

namespace Duha.SIMS.BAL.Product
{
    public class ProductCategoryProcess : SIMSBalOdataBase<ProductCategorySM>
    {
        #region Properties
        private readonly ILoginUserDetail _loginUserDetail;
        #endregion Properties

        #region Constructor
        public ProductCategoryProcess(IMapper mapper, ILoginUserDetail loginUserDetail, ApiDbContext apiDbContext)
            : base(mapper, apiDbContext)
        {
            _loginUserDetail = loginUserDetail;
        }
        #endregion Constructor

        #region Odata
        public override async Task<IQueryable<ProductCategorySM>> GetServiceModelEntitiesForOdata()
        {
            var entitySet = _apiDbContext.ProductCategories;
            var query = entitySet.Select(entity => new ProductCategorySM
            {
                Name = entity.Name,
                Description = entity.Description,
                ImagePath = ConvertImagePathToBase64(entity.ImagePath),
                CategoryIcon = entity.CategoryIcon,
                Level = (LevelTypeSM)entity.Level,
                LevelId = entity.LevelId,
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

        #region Get All L1 Categories
        /// <summary>
        /// Fetches all the Level1 Categories from the database, with images in base 64 format
        /// </summary>
        /// <returns>
        /// If Successful, Returns List of ProductCategorySM; Otherwise returns null
        /// </returns>
        /// <exception cref="SIMSException"></exception>
        public async Task<List<ProductCategorySM>?> GetAllLevel1Categories()
        {
            try
            {
                // Retrieve all product categories from the database
                var itemsFromDb = await _apiDbContext.ProductCategories.
                    Where(c => c.Level == LevelTypeDM.Level1 && c.Name != "Demo Category")
                    .ToListAsync();

                // If no product categories are found, return null
                if (itemsFromDb == null || itemsFromDb.Count == 0)
                {
                    throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, "Level1 Categories Not Found or Level1 Categories does not Exist...");
                }
                return _mapper.Map<List<ProductCategorySM>>(itemsFromDb);
            }
            catch (SIMSException ex)
            {
                // Handle SIMSException
                throw;
            }
            catch (Exception ex)
            {
                // Log the exception and rethrow
                //ILogger(ex, "An error occurred in GetAllLevel1Categories");
                throw new Exception("An unexpected error occurred. Please contact support.");
            }
        }
        #endregion Get All L1 Categories

        #region Get All L2 Categories
        /// <summary>
        /// Fetches All Level2 Categories from the database
        /// </summary>
        /// <returns>
        /// If Successful, Returns List of ProductCategorySM; Otherwise returns null 
        /// </returns>
        /// <exception cref="SIMSException"></exception>
        public async Task<List<ProductCategorySM>?> GetAllLevel2Categories()
        {
            try
            {
                // Retrieve all product categories from the database
                var itemsFromDb = await _apiDbContext.ProductCategories.
                    Where(c => c.Level == LevelTypeDM.Level2 && c.Name != "Demo Category")
                    .ToListAsync();
                // If no product categories are found, return null
                if (itemsFromDb == null || itemsFromDb.Count == 0)
                {
                    throw new SIMSException(ApiErrorTypeSM.InvalidInputData_Log, $"Categories Not Available");
                }
                //return _mapper.Map<List<ProductCategorySM>>(itemsFromDb);
                // Retrieve all images in parallel
                var categoriesWithImages = await GetProductCategoriesWithImages(itemsFromDb);
                foreach (var category in categoriesWithImages)
                {
                    var level3CategoryIds = await _apiDbContext.ProductCategories
                        .Where(l => l.Level == LevelTypeDM.Level3 && l.LevelId == category.Id)
                        .Select(l => l.Id)
                        .ToListAsync();
                    var productsWithLevel3CategoriesCount = await _apiDbContext.Products
                        .Where(p => level3CategoryIds.Contains(p.ProductCategoryId))
                        .CountAsync();
                    category.ProductCount = productsWithLevel3CategoriesCount;
                }
                return categoriesWithImages;
            }
            catch (SIMSException ex)
            {
                // Handle exceptions and throw SIMSException with appropriate details
                throw;
            }
            catch (Exception ex)
            {
                // Handle exceptions and throw SIMSException with appropriate details
                throw new SIMSException(ApiErrorTypeSM.Fatal_Log, $@"Something went wrong in fetching the level2 categories");
            }
        }
        #endregion Get All L2 Categories

        #region Get All Categories Based on LevelType 
        /// <summary>
        /// Fetches all the Categories from the database based on LevelType, with images in base 64 format
        /// </summary>
        /// <returns>
        /// If Successful, Returns List of ProductCategorySM; Otherwise returns null
        /// </returns>
        /// <exception cref="SIMSException"></exception>
        public async Task<List<ProductCategorySM>?> GetAllCategoriesBasedOnLevelType(LevelTypeSM levelType)
        {
            try
            {
                List<ProductCategorySM> categories = new List<ProductCategorySM>();
                if (levelType == LevelTypeSM.Level1)
                {
                    var level1Categories = await _apiDbContext.ProductCategories.
                    Where(c => c.Level == LevelTypeDM.Level1 && c.Name != "Demo Category")
                    .ToListAsync();
                    categories = _mapper.Map<List<ProductCategorySM>>(level1Categories);
                }
                else if (levelType == LevelTypeSM.Level2)
                {
                    var level2Categories = await _apiDbContext.ProductCategories.
                    Where(c => c.Level == LevelTypeDM.Level2 && c.Name != "Demo Category")
                    .ToListAsync();

                    categories = _mapper.Map<List<ProductCategorySM>>(level2Categories);
                }
                else if (levelType == LevelTypeSM.Level3)
                {
                    var level3Categories = await _apiDbContext.ProductCategories.
                   Where(c => c.Level == LevelTypeDM.Level3 && c.Name != "Demo Category")
                   .ToListAsync();
                    categories = _mapper.Map<List<ProductCategorySM>>(level3Categories);
                }
                return categories;
            }
            catch (SIMSException ex)
            {
                // Handle SIMSException
                throw;
            }
            catch (Exception ex)
            {
                // Log the exception and rethrow
                //ILogger(ex, "An error occurred in GetAllLevel1Categories");
                throw new SIMSException(ApiErrorTypeSM.Fatal_Log,"An unexpected error occurred. Please contact support.");
            }
        }
        #endregion Get All Categories Based on LevelType

        #region Get By Id
        /// <summary>
        /// Retrieves a specific product category with its image base64 encoded.
        /// </summary>
        /// <param name="Id">The Id of the product category to retrieve.</param>
        /// <returns>
        /// If successful, returns a ProductCategorySM; otherwise, returns null.
        /// </returns>
        public async Task<ProductCategorySM?> GetProductCategoryById(int Id)
        {
            // Retrieve the product category from the database by its Id
            var singleItemFromDb = await _apiDbContext.ProductCategories.FindAsync(Id);

            // If no blog is found with the specified Id, return null
            if (singleItemFromDb == null)
                return null;

            try
            {
                // Convert the image to base64
                var base64Image = await ConvertToBase64(singleItemFromDb.ImagePath);

                // If conversion is successful, update the image and return the mapped product category
                //if (base64Image != null)
                {
                    singleItemFromDb.ImagePath = base64Image;
                    return _mapper.Map<ProductCategorySM>(singleItemFromDb);
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions and throw SIMSException with appropriate details
                throw new SIMSException(ApiErrorTypeSM.Fatal_Log, "Something went wrong in fetching the product category");
            }

        }
        #endregion Get By Id

        #region Add
        /// <summary>
        /// Adds a new product category to the database.
        /// </summary>
        /// <param name="productCategorySM">The ProductCategorySM object representing the new product category.</param>
        /// <returns>
        /// If successful, returns the added ProductCategorySM; otherwise, returns null.
        /// </returns>
        public async Task<ProductCategorySM?> AddProductCategory(ProductCategorySM productCategorySM)
        {
            string? productCatImageRelativePath = null;

            // Check if productCategorySM is null, if so, return null
            if (productCategorySM == null)
                return null;

            try
            {
                // Map ProductCategorySM to ProductCategoryDM and set creator information
                var productCategoryDM = _mapper.Map<ProductCategoryDM>(productCategorySM);
                productCategoryDM.CreatedBy = _loginUserDetail.LoginId;
                productCategoryDM.CreatedOnUTC = DateTime.UtcNow;

                // Save the image and get its full path
                productCatImageRelativePath = await SaveFromBase64(productCategorySM.ImagePath);

                if (productCatImageRelativePath != null)
                {
                    productCategoryDM.ImagePath = productCatImageRelativePath;

                    // Add product category to the database
                    await _apiDbContext.ProductCategories.AddAsync(productCategoryDM);

                    // If save changes is successful, return the saved product category
                    if (await _apiDbContext.SaveChangesAsync() > 0)
                    {
                        return _mapper.Map<ProductCategorySM>(productCategoryDM);
                    }

                    // If save changes is not successful, return null;
                    return null;
                }
            }
            catch (Exception ex)
            {
                // If an exception occurs, delete the image file (if it was saved)
                if (productCatImageRelativePath != null)
                {
                    string fullImagePath = Path.GetFullPath(productCatImageRelativePath);
                    if (File.Exists(fullImagePath))
                        File.Delete(fullImagePath);
                }
                throw new SIMSException(ApiErrorTypeSM.NoRecord_NoLog, @"Error while adding a new Product Category");
            }
            return null;
        }
        #endregion Add

        #region Add Category With Level Checks
        /// <summary>
        /// Adds product Category with proper handlingin the databse 
        /// </summary>
        /// <param name="productCategorySM"> productCategorySM object to add in the databse</param>
        /// <returns>
        /// If Successful, Returns a ProductCategorySM; Otherwise returns null
        /// </returns>
        /// <exception cref="SIMSException"></exception>
        public async Task<ProductCategorySM?> AddCategoryWithLevelChecks(ProductCategorySM productCategorySM)
        {
            string? productCatImageRelativePath = null;

            // Check if productCategorySM is null, if so, return null
            if (productCategorySM == null || productCategorySM.Level == null || productCategorySM.Level <= 0)
                throw new SIMSException(ApiErrorTypeSM.Fatal_Log, "Kindly Provide necessary details to Add New Category");

            try
            {
                // Check if Level is Level1 and ensure LevelId is null
                if (productCategorySM.Level == LevelTypeSM.Level1 && productCategorySM.LevelId.HasValue)
                {
                    //throw new ArgumentException("LevelId must be null for Level1");
                    throw new SIMSException(ApiErrorTypeSM.Fatal_Log, "LevelId must be null for Level1");
                }

                // Check if Level is Level2 and ensure LevelId is valid
                if (productCategorySM.Level == LevelTypeSM.Level2)
                {
                    if (!productCategorySM.LevelId.HasValue)
                    {
                        //throw new ArgumentException("LevelId must be provided for Level2");
                        throw new SIMSException(ApiErrorTypeSM.Fatal_Log, "LevelId must be provided for Level2");
                    }

                    // Check if the provided LevelId exists in the database
                    var existingCategory = await _apiDbContext.ProductCategories
                        .FirstOrDefaultAsync(pc => pc.Id == productCategorySM.LevelId);

                    if (existingCategory == null)
                    {
                        //throw new ArgumentException("Invalid LevelId for Level2");
                        throw new SIMSException(ApiErrorTypeSM.Fatal_Log, "Invalid LevelId for Level2");
                    }
                    // Check if the level of the existing category is Level1
                    if (existingCategory.Level != LevelTypeDM.Level1)
                    {
                        //throw new ArgumentException("Invalid LevelId for Level2. Level1 category expected.");
                        throw new SIMSException(ApiErrorTypeSM.Fatal_Log, "Invalid LevelId for Level2. Level1 category expected.");
                    }
                }

                // Check if Level is Level3 and ensure LevelId is valid
                if (productCategorySM.Level == LevelTypeSM.Level3)
                {
                    if (!productCategorySM.LevelId.HasValue)
                    {
                        //throw new ArgumentException("LevelId must be provided for Level3");
                        throw new SIMSException(ApiErrorTypeSM.Fatal_Log, "LevelId must be provided for Level3");
                    }

                    // Check if the provided LevelId exists in the database
                    var existingCategory = await _apiDbContext.ProductCategories
                        .FirstOrDefaultAsync(pc => pc.Id == productCategorySM.LevelId);

                    if (existingCategory == null)
                    {
                        //throw new ArgumentException("Invalid LevelId for Level3");
                        throw new SIMSException(ApiErrorTypeSM.Fatal_Log, "Invalid LevelId for Level3");
                    }

                    // Check if the level of the existing category is Level2
                    if (existingCategory.Level != LevelTypeDM.Level2)
                    {
                        //throw new ArgumentException("Invalid LevelId for Level3. Level2 category expected.");
                        throw new SIMSException(ApiErrorTypeSM.Fatal_Log, "Invalid LevelId for Level3. Level2 category expected.");
                    }
                }

                // Map ProductCategorySM to ProductCategoryDM and set creator information
                var productCategoryDM = _mapper.Map<ProductCategoryDM>(productCategorySM);
                productCategoryDM.CreatedBy = _loginUserDetail.LoginId;
                productCategoryDM.CreatedOnUTC = DateTime.UtcNow;
                // Save the image and get its full path

                if (!string.IsNullOrEmpty(productCategorySM.ImagePath))
                {
                    // Save the image and get its full path
                    productCatImageRelativePath = await SaveFromBase64(productCategorySM.ImagePath);
                    productCategoryDM.ImagePath = productCatImageRelativePath;
                }

                // Add product category to the database
                await _apiDbContext.ProductCategories.AddAsync(productCategoryDM);

                // If save changes is successful, return the saved product category
                if (await _apiDbContext.SaveChangesAsync() > 0)
                {
                    return _mapper.Map<ProductCategorySM>(productCategoryDM);
                }

                // If save changes is not successful, return null;
                return null;

            }
            catch (Exception ex)
            {
                // If an exception occurs, delete the image file (if it was saved)
                if (productCatImageRelativePath != null)
                {
                    string fullImagePath = Path.GetFullPath(productCatImageRelativePath);
                    if (File.Exists(fullImagePath))
                        File.Delete(fullImagePath);
                }
                throw new SIMSException(ApiErrorTypeSM.NoRecord_NoLog, @"Something went wrong while adding new Category");
            }
        }


        #endregion Add Category With Level Checks

        #region AddProductsInCategory
        /// <summary>
        /// Add Product In the Product Category
        /// </summary>
        /// <param name="categoryId">Category id, to which we need to add product</param>
        /// <param name="productId">ProductId, the product we need to add in the Product Category</param>
        /// <returns>
        /// If Successful, Returns a ProductSM; Otherwise returns null
        /// </returns>
        /// <exception cref="SIMSException"></exception>
        public async Task<ProductSM?> AddProductToExistingCategory(int categoryId, int productId)
        {
            if (categoryId <= 0 || productId <= 0)
                return null;

            var existingCategory = await _apiDbContext.ProductCategories.FindAsync(categoryId);
            var existingProduct = await _apiDbContext.Products.FindAsync(productId);

            if (existingCategory == null || existingProduct == null)
            {
                return null;
            }
            if (existingCategory.Level != LevelTypeDM.Level3)
            {
                throw new SIMSException(ApiErrorTypeSM.Fatal_Log, "Provide Level3 Category");
            }
            existingProduct.ProductCategory = existingCategory;
            if (_apiDbContext.SaveChanges() > 0)
            {
                return _mapper.Map<ProductSM>(existingProduct);
            }

            return null;
        }


        #endregion AddProductsInCategory

        #region Update
        /// <summary>
        /// Updates a product category in the database.
        /// </summary>
        /// <param name="objIdToUpdate">The Id of the product category to update.</param>
        /// <param name="productCategorySM">The updated ProductCategorySM object.</param>
        /// <returns>
        /// If successful, returns the updated ProductCategorySM; otherwise, returns null.
        /// </returns>
        public async Task<ProductCategorySM?> UpdateProductCategory(int objIdToUpdate, ProductCategorySM productCategorySM)
        {
            if (productCategorySM != null && objIdToUpdate > 0)
            {
                // Retrieve target product category to update from db
                ProductCategoryDM? objDM = await _apiDbContext.ProductCategories.FindAsync(objIdToUpdate);

                if (objDM != null)
                {
                    var imageFullPath = "";
                    if (!string.IsNullOrEmpty(objDM.ImagePath))
                    {
                        imageFullPath = Path.GetFullPath(objDM.ImagePath);
                    }
                    // Get product category image full path

                    // Preserve the existing values of Level and LevelId
                    productCategorySM.Id = objDM.Id;
                    productCategorySM.Level = (LevelTypeSM)objDM.Level;
                    productCategorySM.LevelId = objDM.LevelId;

                    // Update other properties using AutoMapper
                    _mapper.Map(productCategorySM, objDM);

                    if (string.IsNullOrEmpty(productCategorySM.ImagePath))
                    {
                        // If ImagePath is null in the input, update it to null
                        objDM.ImagePath = null;
                    }
                    else
                    {
                        // Convert base 64 string to image and store it inside a folder
                        // Return the relative path of the image
                        var imageRelativePath = await SaveFromBase64(imageFullPath);

                        if (imageRelativePath != null)
                        {
                            objDM.ImagePath = imageRelativePath;
                        }
                    }

                    objDM.LastModifiedBy = _loginUserDetail.LoginId;
                    objDM.LastModifiedOnUTC = DateTime.UtcNow;

                    if (await _apiDbContext.SaveChangesAsync() > 0)
                    {
                        // Delete the previous image from the folder
                        if (File.Exists(imageFullPath))
                            File.Delete(imageFullPath);

                        return _mapper.Map<ProductCategorySM>(objDM);
                    }
                    return null;
                }
                else
                {
                    throw new SIMSException(ApiErrorTypeSM.Fatal_Log, "Product category to update not found, add as new instead.");
                }
            }
            return null;
        }


        #endregion Update

        /*#region Remove Category Without Effecting Other Categories or Models
        /// <summary>
        /// Deletes Category by Id and also associated Categories as well
        /// </summary>
        /// <param name="id">Category Id to delete</param>
        /// <returns></returns>
        public async Task<DeleteResponseRoot> DeleteProductCategory(int id)
        {
            try
            {
                using (var transaction = _apiDbContext.Database.BeginTransaction())
                {
                    var categoryToDelete = await _apiDbContext.ProductCategories.FindAsync(id);
                    var imagePath = "";
                    if (categoryToDelete != null)
                    {
                        if (categoryToDelete.ImagePath != null)
                        {
                            imagePath = categoryToDelete.ImagePath;
                        }

                        await DeleteAssociatedProductsAndCategories(categoryToDelete);

                        _apiDbContext.ProductCategories.Remove(categoryToDelete);

                        if (await _apiDbContext.SaveChangesAsync() > 0)
                        {
                            *//* if (File.Exists(Path.GetFullPath(imagePath)))
                                 File.Delete(imagePath);*//*

                            transaction.Commit();
                            return new DeleteResponseRoot(true, $"Product category with Id {id} deleted successfully!");
                        }
                    }

                    // If no product category with the specified Id is found, return a failure response
                    return new DeleteResponseRoot(false, "Product category not found");
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                return new DeleteResponseRoot(false, $"Error deleting product category with Id {id}");
            }
        }

        private async Task DeleteAssociatedProductsAndCategories(ProductCategoryDM categoryToDelete)
        {
            if (categoryToDelete.Level == LevelTypeDM.Level1)
            {
                await DeleteLevel1Category(categoryToDelete);
            }
            else if (categoryToDelete.Level == LevelTypeDM.Level2)
            {
                await DeleteLevel2Category(categoryToDelete);
            }
            else if (categoryToDelete.Level == LevelTypeDM.Level3)
            {
                await DeleteLevel3Category(categoryToDelete);
            }
        }

        private async Task DeleteLevel1Category(ProductCategoryDM categoryToDelete)
        {
            try
            {
                var level2Categories = await _apiDbContext.ProductCategories
                    .Where(c => c.LevelId == categoryToDelete.Id)
                    .ToListAsync();
                //can be deleted
               

                if (level2Categories != null || level2Categories.Count != 0)
                {
                    foreach (var level2Category in level2Categories)
                    {
                        level2Category.LevelId = 19;
                    }

                    await _apiDbContext.SaveChangesAsync();
                }

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task DeleteLevel2Category(ProductCategoryDM categoryToDelete)
        {
            try
            {
                var level3Categories = await _apiDbContext.ProductCategories
                .Where(c => c.LevelId == categoryToDelete.Id)
                .ToListAsync();
                #region LINQ Approch
                if (level3Categories != null || level3Categories.Count != 0)
                {
                    foreach (var category in level3Categories)
                    {
                        category.LevelId = 20;
                    }
                    await _apiDbContext.SaveChangesAsync();
                }

                #endregion LINQ Approach
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        private async Task DeleteLevel3Category(ProductCategoryDM categoryToDelete)
        {
            try
            {
                var productsToUpdate = await _apiDbContext.Products
               .Where(p => p.ProductCategoryId == categoryToDelete.Id)
               .ToListAsync();
                if (productsToUpdate != null || productsToUpdate.Count != 0)
                {
                    foreach (var product in productsToUpdate)
                    {
                        product.ProductCategoryId = 21;

                    }
                    await _apiDbContext.SaveChangesAsync();
                }

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        #endregion Remove Category Without Effecting Other Categories or Models*/

        #region Private Functions
        /// <summary>
        /// Saves a base64 encoded string as a jpg/jpeg/png etc file on the server.
        /// </summary>
        /// <param name="base64String">The base64 encoded string of the jpg/jpeg/png etc.</param>
        /// <returns>
        /// If successful, returns the relative file path of the saved file; 
        /// otherwise, returns null.
        /// </returns>
        private async Task<string?> SaveFromBase64(string base64String)
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
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot\content\productCategories");

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
        private async Task<string?> ConvertToBase64(string filePath)
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
                return null;
            }
        }

        #endregion Private Functions

        #region GetProductCategoryWithImages
        /// <summary>
        /// Fetches ProductCategories with Image bas64 encoded
        /// </summary>
        /// <param name="categoryList">List CategoryListDM Object to which we need to change it to ProductCategorySM with images base64 encoded </param>
        /// <returns>
        /// If Successful, Returns List of ProductCategorySM otherwise returns null
        /// </returns>
        public async Task<List<ProductCategorySM>> GetProductCategoriesWithImages(List<ProductCategoryDM> categoryList)
        {
            var tasks = categoryList
                .Select(async category =>
                {
                    // Convert the image to base64
                    var base64Image = await ConvertToBase64(category.ImagePath);

                    // If conversion is successful, update the image and return the mapped product category
                    if (base64Image != null)
                    {
                        category.ImagePath = base64Image;
                        return _mapper.Map<ProductCategorySM>(category);
                    }

                    return null;
                });

            var categories = await Task.WhenAll(tasks);

            // Filter out any null results (in case image conversion failed)
            var validCategories = categories.Where(app => app != null);
            return validCategories.ToList();
        }
        #endregion GetProductcategoryWithImages

        #region GetProductCategoryWithImages
        /// <summary>
        /// Fetches ProductCategories with Image bas64 encoded
        /// </summary>
        /// <param name="categoryList">List CategoryListDM Object to which we need to change it to ProductCategorySM with images base64 encoded </param>
        /// <returns>
        /// If Successful, Returns List of ProductCategorySM otherwise returns null
        /// </returns>
        public async Task<List<ProductCategorySM>> GetProductCategoriesWithImagesIQ(IQueryable<ProductCategorySM> IquarableList)
        {
            List<ProductCategorySM> categoryList = IquarableList.ToList();
            var tasks = categoryList
                .Select(async category =>
                {
                    // Convert the image to base64
                    var base64Image = await ConvertToBase64(category.ImagePath);

                    // If conversion is successful, update the image and return the mapped product category
                    if (base64Image != null)
                    {
                        category.ImagePath = base64Image;
                        return _mapper.Map<ProductCategorySM>(category);
                    }

                    return null;
                });

            var categories = await Task.WhenAll(tasks);

            // Filter out any null results (in case image conversion failed)
            var validCategories = categories.Where(app => app != null);
            return validCategories.ToList();

        }
        #endregion GetProductcategoryWithImages
    }
}
