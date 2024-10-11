using AutoMapper;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Vml.Office;
using Duha.SIMS.BAL.Base;
using Duha.SIMS.BAL.Exceptions;
using Duha.SIMS.DAL.Contexts;
using Duha.SIMS.DomainModels.Enums;
using Duha.SIMS.DomainModels.Product;
using Duha.SIMS.ServiceModels.CommonResponse;
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
                CategoryId = (int)entity.CategoryId,
                BrandId = entity.BrandId,
                UnitId = entity.UnitId,
                Id = entity.Id,
                CreatedBy = entity.CreatedBy,
                LastModifiedBy = entity.LastModifiedBy,
                CreatedOnUTC = entity.CreatedOnUTC,
                LastModifiedOnUTC = entity.LastModifiedOnUTC,
            });

            // Return the projected query as IQueryable
            return await Task.FromResult(query);
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
                .OrderByDescending(c => c.CreatedOnUTC)
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
                var sm = await GetProductDetailsById(item.Id);
                response.AddRange(sm);
            }
            return response;
        }

        public async Task<int> GetAllProductsCount()
        {
            var count =  _apiDbContext.ProductDetails.AsNoTracking().Count();
            return count;
        }

        #endregion Get All

        #region Get All and By Id Product Details

        #region Get By ProductId
        /// <summary>
        /// Fetches the the Product from the database, with images in base 64 format
        /// </summary>
        /// <returns>
        /// If Successful, Returns ProductSM otherwise return null
        /// </returns>
        public async Task<ProductSM> GetProductsBasedOnProductDetailId(int id)
        {
            var item = await _apiDbContext.ProductDetails.Where(x => x.Id == id)
                .FirstOrDefaultAsync();
            // Return an empty list instead of null if there are no items
            if (item == null)
            {
                return null;
            }
            var dm = await _apiDbContext.Products.FindAsync(item.ProductId);
            var res = new ProductSM()
            {
                Name = dm.Name,
                Id = dm.Id,
                CategoryId = (int)dm.CategoryId,
                BrandId = dm.BrandId,
                UnitId = dm.UnitId,
                WarehouseId = item.WarehouseId,
                SupplierId = item.SupplierId,
                Code = item.Code,
                Quantity = item.Quantity,
                Price = item.Price,
                Image = ConvertImagePathToBase64(item.Image),
                ProductDetailId = item.Id,
                CreatedBy = dm.CreatedBy,
                CreatedOnUTC = dm.CreatedOnUTC,
                LastModifiedBy = item.LastModifiedBy,
                LastModifiedOnUTC = item.LastModifiedOnUTC,
            };
            return res;
        }
        #endregion Get By ProductId

        #region Get All
        /// <summary>
        /// Fetches all the Products from the database, with images in base 64 format
        /// </summary>
        /// <returns>
        /// If Successful, Returns List of ProductSM otherwise return null
        /// </returns>
        public async Task<List<ProductSM>> GetAllProductsDetailsProducts(int skip, int top)
        {
            var itemsFromDb = await _apiDbContext.ProductDetails
                .OrderByDescending(c => c.CreatedOnUTC)
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
                var sm = await GetProductsBasedOnProductDetailId(item.Id);
                response.Add(sm);
            }
            return response;
        }
        #endregion Get All


        #endregion Get All Get All and By Id Product Details

        #region Get Product Details By Id
        /// <summary>
        /// Get Products By Using ProductId
        /// </summary>
        /// <param name="Id">Using Id of Product Fetches the respective Products</param>
        /// <returns> 
        /// If Successful, returns List<ProductSM></ProductSM> otherwise returns Null.
        /// </returns>
        /// <exception cref="SIMSException"></exception>
        public async Task<List<ProductSM>> GetProductDetailsById(int Id)
        {
            var dm = await _apiDbContext.Products.FindAsync(Id);

            if (dm == null)
            {
                return null;
            }

            var detailsList = await _apiDbContext.ProductDetails.Where(x=>x.ProductId == Id)
                .OrderByDescending(c => c.CreatedOnUTC)
                .ToListAsync();
            var productList = new List<ProductSM>();
            foreach (var item in detailsList)
            {
                var res = new ProductSM()
                {
                    Name = dm.Name,
                    Id = dm.Id,
                    CategoryId = (int)dm.CategoryId,
                    BrandId = dm.BrandId,
                    UnitId = dm.UnitId,
                    WarehouseId = item.WarehouseId,
                    SupplierId = item.SupplierId,
                    Code = item.Code,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    Image = ConvertImagePathToBase64(item.Image),
                    ProductDetailId = item.Id,
                    CreatedBy = dm.CreatedBy,
                    CreatedOnUTC = dm.CreatedOnUTC,
                    LastModifiedBy = item.LastModifiedBy,
                    LastModifiedOnUTC = item.LastModifiedOnUTC,
                };
                productList.Add(res);
            }
            return productList;
        }
        #endregion Get Product Details By Id

        #region Get Suppliers products
        /// <summary>
        /// Fetches products of a particular supplier
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<List<ProductSM>> GetProductsBySupplierId(int supplierId)
        {
            // Get the list of products and their corresponding details for the given supplierId
            var supplierProductDetails = await _apiDbContext.ProductDetails
                .Where(pd => pd.SupplierId == supplierId)
                .Include(pd => pd.Products)
                .OrderByDescending(c => c.CreatedOnUTC)
                .ToListAsync();

            // Map the result to ProductSM
            var productList = supplierProductDetails.Select(item => new ProductSM
            {
                Id = item.Products.Id,
                Name = item.Products.Name,
                CategoryId = item.Products.CategoryId,
                BrandId = item.Products.BrandId,
                UnitId = item.Products.UnitId,
                WarehouseId = item.WarehouseId,
                SupplierId = item.SupplierId,
                Code = item.Code,
                Quantity = item.Quantity,
                Price = item.Price,
                Image = ConvertImagePathToBase64(item.Image),
                ProductDetailId = item.Id,
                CreatedBy = item.Products.CreatedBy,
                CreatedOnUTC = item.Products.CreatedOnUTC,
                LastModifiedBy = item.LastModifiedBy,
                LastModifiedOnUTC = item.LastModifiedOnUTC
            }).ToList();

            return productList;
        }


        #endregion Get Suppliers products

        #region Products in Category

        public async Task<List<ProductSM>> GetProductsByCategoryIdAsync(int categoryId)
        {
            // Initialize categoryIds list to store Level 2 category IDs if applicable
            List<int> categoryIds = null;

            // Check if the given category is a Level 1 category
            var level1Category = await _apiDbContext.ProductCategories
                .FirstOrDefaultAsync(x => x.Id == categoryId && x.Level == CategoryLevelDM.Level1);

            if (level1Category != null)
            {
                // If it's a Level 1 category, retrieve associated Level 2 category IDs
                categoryIds = await _apiDbContext.ProductCategories
                    .Where(x => x.LevelId == categoryId)
                    .Select(x => x.Id)
                    .ToListAsync();
            }

            // Check if the given category is a Level 2 category
            var level2Category = await _apiDbContext.ProductCategories
                .FirstOrDefaultAsync(x => x.Id == categoryId && x.Level == CategoryLevelDM.Level2);

            if (level2Category != null)
            {
                // If it's a Level 2 category, use its ID directly
                categoryIds = new List<int> { level2Category.Id };
            }

            // If no valid category was found, return a user-friendly error
            if (categoryIds == null || !categoryIds.Any())
            {
                throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog,
                    "The specified category was not found or it does not have any associated subcategories. Please verify the category and try again.");
            }

            // Fetch the products corresponding to the category and map to the service model
            var productDetails = await _apiDbContext.ProductDetails
                .Where(pd => categoryIds.Contains(pd.Products.CategoryId))
                .Include(pd => pd.Products)
                .OrderByDescending(pd => pd.CreatedOnUTC)
                .ToListAsync();

            // If no products were found for the given category, return a message
            if (!productDetails.Any())
            {
                throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog,
                    "No products were found for the selected category. Please try another category.");
            }

            // Map the fetched products to the ProductSM model
            var productList = productDetails.Select(item => new ProductSM
            {
                Id = item.Products.Id,
                Name = item.Products.Name,
                CategoryId = item.Products.CategoryId,
                BrandId = item.Products.BrandId,
                UnitId = item.Products.UnitId,
                WarehouseId = item.WarehouseId,
                SupplierId = item.SupplierId,
                Code = item.Code,
                Quantity = item.Quantity,
                Price = item.Price,
                Image = ConvertImagePathToBase64(item.Image),
                ProductDetailId = item.Id,
                CreatedBy = item.Products.CreatedBy,
                CreatedOnUTC = item.Products.CreatedOnUTC,
                LastModifiedBy = item.LastModifiedBy,
                LastModifiedOnUTC = item.LastModifiedOnUTC
            }).ToList();

            return productList;
        }


        #endregion Products in Category

        #region Products in Warehouse

        public async Task<List<ProductSM>> GetProductsByWarehouseId(int warehouseId)
        {
            // Get the list of products and their corresponding details for the given supplierId
            var warehouseproducts = await _apiDbContext.ProductDetails
                .Where(pd => pd.WarehouseId == warehouseId)
                .Include(pd => pd.Products)
                .OrderByDescending(c => c.CreatedOnUTC)
                .ToListAsync();

            // Map the result to ProductSM
            var productList = warehouseproducts.Select(item => new ProductSM
            {
                Id = item.Products.Id,
                Name = item.Products.Name,
                CategoryId = item.Products.CategoryId,
                BrandId = item.Products.BrandId,
                UnitId = item.Products.UnitId,
                WarehouseId = item.WarehouseId,
                SupplierId = item.SupplierId,
                Code = item.Code,
                Quantity = item.Quantity,
                Price = item.Price,
                Image = ConvertImagePathToBase64(item.Image),
                ProductDetailId = item.Id,
                CreatedBy = item.Products.CreatedBy,
                CreatedOnUTC = item.Products.CreatedOnUTC,
                LastModifiedBy = item.LastModifiedBy,
                LastModifiedOnUTC = item.LastModifiedOnUTC
            }).ToList();

            return productList;
        }

        #endregion Products in Warehouse

        #region Get product by ProductId and productDetailid

        /// <summary>
        /// Get Product By Using ProductId
        /// </summary>
        /// <param name="Id">Using Id of Product Fetches the respective Product</param>
        /// <returns> 
        /// If Successful, returns ProductSM otherwise returns Null.
        /// </returns>
        public async Task<ProductSM> GetProductDetailsByIdAndProductDetailId(int productId, int productDetailid)
        {
            var dm = await _apiDbContext.Products.FindAsync(productId);

            if (dm == null)
            {
                return null;
            }

            var productDetail = await _apiDbContext.ProductDetails.Where(x => x.ProductId == productId && x.Id == productDetailid).FirstOrDefaultAsync();
            
            var res = new ProductSM()
            {
                Id = dm.Id,
                Name = dm.Name,
                CategoryId = (int)dm.CategoryId,
                BrandId = dm.BrandId,
                UnitId = dm.UnitId,
                WarehouseId = productDetail.WarehouseId,
                SupplierId = productDetail.SupplierId,
                Code = productDetail.Code,
                Quantity = productDetail.Quantity,
                Price = productDetail.Price,
                Image = ConvertImagePathToBase64(productDetail.Image),
                ProductDetailId = productDetail.Id,
                CreatedBy = dm.CreatedBy,
                CreatedOnUTC = dm.CreatedOnUTC,
                LastModifiedBy = productDetail.LastModifiedBy,
                LastModifiedOnUTC = productDetail.LastModifiedOnUTC,
            };
            return res;
        }

        #endregion Get product by ProductId and productDetailid

        #region Add
        /// <summary>
        /// Adds a new Product to the database.
        /// </summary>
        /// <param name="ProductSM">The ProductSM object representing the new Product.</param>
        /// <returns>
        /// If successful, returns the added ProductSM; otherwise, returns null.
        /// </returns>
        public async Task<ProductSM?> AddProduct(CreateProductSM objSM/*ProductSM objSM, ProductVariantDetailsSM variants*/)
        {
            if (objSM.Product == null)
            {
                return null;
            }

            string? productImageRelativePath = null;
            var existingProduct = await _apiDbContext.Products
                .Where(x => x.Name == objSM.Product.Name &&
                            x.CategoryId == objSM.Product.CategoryId &&
                            x.BrandId == objSM.Product.BrandId &&
                            x.UnitId == objSM.Product.UnitId)
                .FirstOrDefaultAsync();

            var productEntity = new ProductDM()
            {
                Name = objSM.Product.Name,
                CategoryId = objSM.Product.CategoryId,
                BrandId = objSM.Product.BrandId,
                UnitId = objSM.Product.UnitId,
                CreatedBy = objSM.Product.CreatedBy,
                LastModifiedBy = objSM.Product.LastModifiedBy,
                CreatedOnUTC = objSM.Product.CreatedOnUTC,
                LastModifiedOnUTC = objSM.Product.LastModifiedOnUTC
            };

            using var transaction = await _apiDbContext.Database.BeginTransactionAsync();

            if (existingProduct == null)
            {
                await _apiDbContext.Products.AddAsync(productEntity);
                await _apiDbContext.SaveChangesAsync();
                if (objSM.ProductVariantDetails.Count > 0)
                {
                    var productVariantDetails = new List<ProductVariantDM>();
                    foreach(var item in objSM.ProductVariantDetails)
                    {
                        var dm = new ProductVariantDM()
                        {
                            ProductId = productEntity.Id,
                            VariantLevel1Id = item.VariantLevel1Id,
                            VariantLevel2Id = item.VariantLevel2Id,
                            CreatedBy = _loginUserDetail.LoginId,
                            CreatedOnUTC = DateTime.UtcNow
                        };
                        productVariantDetails.Add(dm);
                    }
                    await _apiDbContext.ProductVariants.AddRangeAsync(productVariantDetails);
                    await _apiDbContext.SaveChangesAsync();
                }

                if (!string.IsNullOrEmpty(objSM.Product.Image))
                {
                    productImageRelativePath = await SaveFromBase64(objSM.Product.Image);
                }

                var productDetails = new ProductDetailsDM()
                {
                    Code = objSM.Product.Code,
                    Price = objSM.Product.Price,
                    Quantity = objSM.Product.Quantity,
                    ProductId = productEntity.Id,
                    SupplierId = objSM.Product.SupplierId,
                    WarehouseId = objSM.Product.WarehouseId,
                    CreatedBy = objSM.Product.CreatedBy,
                    Image = productImageRelativePath,
                    LastModifiedBy = objSM.Product.LastModifiedBy,
                    CreatedOnUTC = objSM.Product.CreatedOnUTC,
                    LastModifiedOnUTC = objSM.Product.LastModifiedOnUTC
                };

                await _apiDbContext.ProductDetails.AddAsync(productDetails);
                await _apiDbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                objSM.Product.Id = productEntity.Id;
                objSM.Product.ProductDetailId = productDetails.Id; // Capture the ID after save
                objSM.Product.Image = ConvertImagePathToBase64(productDetails.Image);
                return objSM.Product;
            }
            else
            {
                int productDetailId;
                var existingDetails = await _apiDbContext.ProductDetails
                    .Where(x => x.SupplierId == objSM.Product.SupplierId &&
                                x.Price == objSM.Product.Price &&
                                x.Code == objSM.Product.Code)
                    .FirstOrDefaultAsync();

                if (existingDetails != null)
                {
                    existingDetails.Quantity += objSM.Product.Quantity;
                    existingDetails.LastModifiedBy = _loginUserDetail.LoginId;
                    existingDetails.LastModifiedOnUTC = DateTime.UtcNow;

                    _apiDbContext.ProductDetails.Update(existingDetails);
                    await _apiDbContext.SaveChangesAsync(); // Save changes before getting ID

                    productDetailId = existingDetails.Id; // Assign after save
                }
                else
                {
                    var newProductDetails = new ProductDetailsDM()
                    {
                        Code = objSM.Product.Code,
                        Price = objSM.Product.Price,
                        Quantity = objSM.Product.Quantity,
                        ProductId = existingProduct.Id,
                        SupplierId = objSM.Product.SupplierId,
                        WarehouseId = objSM.Product.WarehouseId,
                        CreatedBy = objSM.Product.CreatedBy,
                        Image = productImageRelativePath,
                        LastModifiedBy = objSM.Product.LastModifiedBy,
                        CreatedOnUTC = objSM.Product.CreatedOnUTC,
                        LastModifiedOnUTC = objSM.Product.LastModifiedOnUTC
                    };

                    await _apiDbContext.ProductDetails.AddAsync(newProductDetails);
                    await _apiDbContext.SaveChangesAsync();
                    productDetailId = newProductDetails.Id; // Assign after save
                }


                await transaction.CommitAsync();

                objSM.Product.Id = existingProduct.Id;
                objSM.Product.ProductDetailId = productDetailId; // Set the correct product detail ID
                objSM.Product.Image = ConvertImagePathToBase64(productImageRelativePath);
                return objSM.Product;
            }
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
        public async Task<ProductSM?> UpdateProductByIdAndProductDetailId(int productDetailId, ProductSM updatedProductSM)
        {
            if (updatedProductSM == null)
            {
                return null;
            }
            var productDetailEntity = await _apiDbContext.ProductDetails
                .Where(x => x.Id == productDetailId)
                .FirstOrDefaultAsync();
            if (productDetailEntity == null)
            {
                return null; // Product detail not found
            }
            // Find the product entity based on the productId
            var productEntity = await _apiDbContext.Products.FindAsync(productDetailEntity.ProductId);
            if (productEntity == null)
            {
                return null; // Product not found
            }

            // Begin a transaction to ensure atomicity
            using var transaction = await _apiDbContext.Database.BeginTransactionAsync();

            // Update the main product fields
            productEntity.Name = updatedProductSM.Name;
            productEntity.LastModifiedBy = _loginUserDetail.LoginId;
            productEntity.LastModifiedOnUTC = DateTime.UtcNow;

            _apiDbContext.Products.Update(productEntity);
            productDetailEntity.Quantity = updatedProductSM.Quantity;
            productDetailEntity.Price = updatedProductSM.Price;
            productDetailEntity.LastModifiedBy = _loginUserDetail.LoginId;
            productDetailEntity.LastModifiedOnUTC = DateTime.UtcNow;

            // If a new image is provided, update the image
            if (!string.IsNullOrEmpty(updatedProductSM.Image))
            {
                var productImageRelativePath = await SaveFromBase64(updatedProductSM.Image);
                productDetailEntity.Image = productImageRelativePath;
            }

            _apiDbContext.ProductDetails.Update(productDetailEntity);

            // Save changes for both product and product detail
            await _apiDbContext.SaveChangesAsync();

            // Commit the transaction
            await transaction.CommitAsync();

            // Return the updated product details
            var result = new ProductSM()
            {
                Id = productEntity.Id,
                Name = productEntity.Name,
                CategoryId = (int)productEntity.CategoryId,
                BrandId = productEntity.BrandId,
                UnitId = productEntity.UnitId,
                WarehouseId = productDetailEntity.WarehouseId,
                SupplierId = productDetailEntity.SupplierId,
                Code = productDetailEntity.Code,
                Quantity = productDetailEntity.Quantity,
                Price = productDetailEntity.Price,
                Image = ConvertImagePathToBase64(productDetailEntity.Image),
                ProductDetailId = productDetailEntity.Id,
                CreatedBy = productEntity.CreatedBy,
                CreatedOnUTC = productEntity.CreatedOnUTC,
                LastModifiedBy = productDetailEntity.LastModifiedBy,
                LastModifiedOnUTC = productDetailEntity.LastModifiedOnUTC,
            };

            return result;
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
        public async Task<DeleteResponseRoot> DeleteProductsById(int productDetailId)
        {
            // Check if a Product with the specified Id is present in the database
            var productDetail = await _apiDbContext.ProductDetails.FindAsync(productDetailId);

            if (productDetail != null)
            {
                var product = await _apiDbContext.Products.FindAsync(productDetail.ProductId);
                //retrieve Product image relativePath to delete it from the folder as well
                var Image = productDetail.Image;

                _apiDbContext.ProductDetails.Remove(productDetail);

                // If save changes is successful, return a success response
                if (await _apiDbContext.SaveChangesAsync() > 0)
                {
                    _apiDbContext.Products.Remove(product);
                    await _apiDbContext.SaveChangesAsync();
                    //get full resume path from relative resume path
                    if (File.Exists(Path.GetFullPath(Image)))
                        File.Delete(Image);

                    return new DeleteResponseRoot(true, $"Product deleted successfully!");
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
                return null;
            }
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
        #endregion Private Functions
    }
}
