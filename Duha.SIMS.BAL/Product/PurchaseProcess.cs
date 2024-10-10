using AutoMapper;
using Duha.SIMS.BAL.Base;
using Duha.SIMS.BAL.Exceptions;
using Duha.SIMS.DAL.Contexts;
using Duha.SIMS.DomainModels.Customer;
using Duha.SIMS.DomainModels.Enums;
using Duha.SIMS.DomainModels.Invoice;
using Duha.SIMS.ServiceModels.CommonResponse;
using Duha.SIMS.ServiceModels.Invoice;
using Duha.SIMS.ServiceModels.LoggedInIdentity;

namespace Duha.SIMS.BAL.Product
{
    public class PurchaseProcess : SIMSBalOdataBase<PurchaseHistorySM>
    {
        #region Properties
        private readonly ILoginUserDetail _loginUserDetail;
        private readonly ApiDbContext _apiDbContext; 
        private readonly ProductProcess _productProcess;
        #endregion Properties

        #region Constructor
        public PurchaseProcess(IMapper mapper, ILoginUserDetail loginUserDetail, ApiDbContext apiDbContext, ProductProcess productProcess)
            : base(mapper, apiDbContext)
        {
            _loginUserDetail = loginUserDetail;
            _apiDbContext = apiDbContext;
            _productProcess = productProcess;
        }
        #endregion Constructor

        #region Odata
        /// <summary>
        /// Gets Service Model Entities For OData
        /// </summary>
        /// <returns>
        /// Return IQueryable PurchasesSM
        /// </returns>
        public override async Task<IQueryable<PurchaseHistorySM>> GetServiceModelEntitiesForOdata()
        {
            var entitySet = _apiDbContext.Purchases;
            var query = entitySet.Select(entity => new PurchaseHistorySM
            {
                Id = entity.Id,
                CustomerId = entity.CustomerId,
                ProductDetailsId = entity.ProductDetailsId,
                Quantity = entity.Quantity,
                TotalPrice = entity.TotalPrice,
                PurchaseDate = entity.PurchaseDate,
                CreatedBy = entity.CreatedBy,
                LastModifiedBy = entity.LastModifiedBy,
                CreatedOnUTC = entity.CreatedOnUTC,
                LastModifiedOnUTC = entity.LastModifiedOnUTC,
            });

            // Return the projected query as IQueryable
            return await Task.FromResult(query);
        }


        #endregion Odata

        #region Buy Product
        public async Task<PurchaseDetailsSM> BuyProduct(PurchaseHistorySM objSM)
        {
            using var transaction = await _apiDbContext.Database.BeginTransactionAsync();

            var existingProduct = await _productProcess.GetProductsBasedOnProductDetailId(objSM.ProductDetailsId);
            if (existingProduct == null)
            {
                return null;
            }

            if (existingProduct.Quantity < objSM.Quantity)
            {
                return null;
            }

            var existingCustomer = await _apiDbContext.Customers.FindAsync(objSM.CustomerId);
            if (existingCustomer == null)
            {
                return null;
            }

            var totalPrice = existingProduct.Price * objSM.Quantity;
            existingProduct.Quantity -= objSM.Quantity;

            var purchaseHistory = new PurchaseHistoryDM
            {
                CustomerId = objSM.CustomerId,
                ProductDetailsId = objSM.ProductDetailsId,
                TotalPrice = totalPrice,
                Quantity = objSM.Quantity,
                MoneyPaid = objSM.MoneyPaid,
                PurchaseDate = DateTime.UtcNow,
                CreatedBy = _loginUserDetail.LoginId,
                CreatedOnUTC = DateTime.UtcNow,
            };

            await _apiDbContext.Purchases.AddAsync(purchaseHistory);
            if (objSM.MoneyPaid > 0)
            {
                var moneyTransferHistory = new MoneyTransactionHistoryDM()
                {
                    Amount = objSM.MoneyPaid,
                    DateOfTransaction = DateTime.UtcNow,
                    PaymentMethod = (PaymentMethodTypeDM)objSM.PaymentMethod,
                    CustomerId = objSM.CustomerId,
                    CreatedBy = _loginUserDetail.LoginId,
                    CreatedOnUTC= DateTime.UtcNow
                };
                await _apiDbContext.MoneyTransactions.AddAsync(moneyTransferHistory);
                await _apiDbContext.SaveChangesAsync();
            }
            var pd = await _apiDbContext.ProductDetails.FindAsync(objSM.ProductDetailsId);
            pd.Quantity -= objSM.Quantity;
            _apiDbContext.ProductDetails.Update(pd);

            if (await _apiDbContext.SaveChangesAsync() > 0)
            {
                await transaction.CommitAsync();

                var details = new PurchaseDetailsSM
                {
                    CustomerName = existingCustomer.Name,
                    ProductName = existingProduct.Name,
                    ProductPrice = existingProduct.Price,
                    QuantityPurchased = objSM.Quantity,
                    PurchaseDate = purchaseHistory.PurchaseDate,
                    TotalPrice = purchaseHistory.TotalPrice,
                    MoneyPaid = purchaseHistory.MoneyPaid,
                };

                return details;
            }

            await transaction.RollbackAsync();
            throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, "Something went wrong...Buy Product Again");
        }




        #endregion Buy Product

        #region Update
        /// <summary>
        /// Updates a Purchase in the database.
        /// </summary>
        /// <param name="objIdToUpdate">The Id of the Purchase to update.</param>
        /// <param name="PurchasesSM">The updated PurchasesSM object.</param>
        /// <returns>
        /// If successful, returns the updated PurchasesSM; otherwise, returns null.
        /// </returns>
        public async Task<PurchaseHistorySM?> UpdatePurchase(int objIdToUpdate, PurchaseHistorySM objSM)
        {
            if (objSM != null && objIdToUpdate > 0)
            {
                //retrieve target product category to update from db
                var objDM = await _apiDbContext.Purchases.FindAsync(objIdToUpdate);

                if (objDM != null)
                {
                    objSM.Id = objIdToUpdate;
                    _mapper.Map(objSM, objDM);
                    objDM.LastModifiedBy = _loginUserDetail.LoginId;
                    objDM.LastModifiedOnUTC = DateTime.UtcNow;

                    if (await _apiDbContext.SaveChangesAsync() > 0)
                    {
                        objSM.Id = objDM.Id;
                        var res = objSM;
                        return res;
                    }
                    throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, "Something went wrong while updating changes");
                }
                else
                {
                    throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, "Purchase to update not found, add as new instead.");
                }
            }
            throw new SIMSException(DomainModels.Base.ExceptionTypeDM.FatalLog, "Please provide details to add new Purchase");
        }

        #endregion Update

        #region Delete
        /// <summary>
        /// Deletes a Purchase by its Id from the database.
        /// </summary>
        /// <param name="id">The Id of the Purchase to be deleted.</param>
        /// <returns>
        /// A DeleteResponseRoot indicating whether the deletion was successful or not.
        /// </returns>
        public async Task<DeleteResponseRoot> DeletePurchasesById(int id)
        {
            // Check if a Purchase with the specified Id is present in the database
            var itemToDelete = await _apiDbContext.Purchases.FindAsync(id);

            if (itemToDelete != null)
            {

                _apiDbContext.Purchases.Remove(itemToDelete);

                if (await _apiDbContext.SaveChangesAsync() > 0)
                {
                    return new DeleteResponseRoot(true, $"Purchase with Id {id} deleted successfully!");
                }
            }
            // If no Purchase with the specified Id is found, return a failure response
            return new DeleteResponseRoot(false, "Purchase not found");
        }
        #endregion Delete
    }
}
