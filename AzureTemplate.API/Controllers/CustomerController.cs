using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AzureTemplate.API.Utility;
using AzureTemplate.Database;
using Microsoft.EntityFrameworkCore;
using System.Net;
using Azure.Storage.Sas;
using Azure.Storage.Blobs;

namespace AzureTemplate.API.Controllers
{
    public class CustomerController : Controller
    {
        private ApplicationDbContext _db;
        private IConfiguration _configuration;
        public CustomerController(ApplicationDbContext dbContext, IConfiguration configuration)
        {
            _db = dbContext;
            _configuration = configuration;
        }

        [Authorize]
        [HttpPost]
        [Route("api/v1/customer")]
        [ProducesResponseType<string>(StatusCodes.Status200OK)]
        [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
        async public Task<IActionResult> CustomerCreate([FromBody] Common.Customer model)
        {
            string userId = User.GetUserId();

            if (string.IsNullOrEmpty(model.Name))
            {
                return BadRequest("Customer name is required");
            }
            
            Database.Customer customer = new Database.Customer()
            {
                Name = model.Name,
                Email = model.Email,
                Phone = model.Phone,
                BirthDate = model.BirthDate,
                Address = model.Address,
                City = model.City,
                State = model.State,
                Postal = model.Postal,
                Notes = model.Notes,
                ImageBase64 = model.ImageBase64,
                Active = model.Active,
                Gender = model.Gender,
                OwnerId = userId
            };

            _db.Customers.Add(customer);
            await _db.SaveChangesAsync();

            return Ok(customer.Id);
        }

        [Authorize]
        [HttpGet]
        [Route("api/v1/customer/{customerId}")]
        [ProducesResponseType<Common.Customer>(StatusCodes.Status200OK)]
        [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
        async public Task<IActionResult> CustomerGetById(string customerId)
        {
            string userId = User.GetUserId();

            var customer = await _db.Customers.Where(c => c.OwnerId == userId && c.Id == customerId).FirstOrDefaultAsync();
            if (customer == null)
                return BadRequest("Customer not found");

            var response = new Common.Customer()
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email,
                Phone = customer.Phone,
                Address = customer.Address,
                City = customer.City,
                State = customer.State,
                Postal = customer.Postal,
                Notes = customer.Notes,
                BirthDate = customer.BirthDate.HasValue ? customer.BirthDate.Value : null,
                Gender = customer.Gender.Value,
                Active = customer.Active.Value,
                ImageBase64 = customer.ImageBase64
            };

            return Ok(customer);
        }

        [Authorize]
        [HttpPut]
        [Route("api/v1/customer/{customerId}")]
        [ProducesResponseType<string>(StatusCodes.Status200OK)]
        [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
        async public Task<IActionResult> CustomerUpdateById([FromBody] Common.Customer model, string customerId)
        {
            string userId = User.GetUserId();

            if (string.IsNullOrEmpty(model.Name))
            {
                return BadRequest("Customer name is required");
            }

            var customer = await _db.Customers.Where(c => c.OwnerId == userId && c.Id == customerId).FirstOrDefaultAsync();
            if (customer == null)
            {
                return BadRequest("Customer not found");
            }

            customer.Name = model.Name;
            customer.Email = model.Email;
            customer.Phone = model.Phone;
            customer.BirthDate = model.BirthDate;
            customer.Address = model.Address;
            customer.City = model.City;
            customer.State = model.State;
            customer.Postal = model.Postal;
            customer.Notes = model.Notes;
            customer.ImageBase64 = model.ImageBase64;
            customer.Active = model.Active;
            customer.Gender = model.Gender;
            customer.UpdateOn = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return Ok(customer.Id);
        }

        [Authorize]
        [HttpDelete]
        [Route("api/v1/customer/{customerId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
        async public Task<IActionResult> CustomerDeleteById(string customerId)
        {
            string userId = User.GetUserId();

            var customer = await _db.Customers.Where(c => c.OwnerId == userId && c.Id == customerId).FirstOrDefaultAsync();

            if(customer == null)
            {
                return BadRequest("Customer not found");
            }

            _db.Customers.Remove(customer);
            await _db.SaveChangesAsync();
            return Ok();
        }


        [Authorize]
        [HttpPost]
        [Route("api/v1/customers")]
        [ProducesResponseType<Common.SearchResponse<Common.Customer>>(StatusCodes.Status200OK)]
        async public Task<IActionResult> CustomerSearch([FromBody] Common.Search model)
        {
            string userId = User.GetUserId();

            var query = _db.Customers.Where(c => c.OwnerId == userId);

            if (!string.IsNullOrEmpty(model.FilterText))
            {
                query = query.Where(i => i.Name.ToLower().Contains(model.FilterText.ToLower()) ||
                                        i.Email.ToLower().ToLower().Contains(model.FilterText.ToLower()) ||
                                        i.Phone.ToLower().Contains(model.FilterText.ToLower()) ||
                                        i.Address.ToLower().Contains(model.FilterText.ToLower()) ||
                                        i.State.ToLower().Contains(model.FilterText.ToLower()) ||
                                        i.Postal.ToLower().Contains(model.FilterText.ToLower()) ||
                                        i.Notes.ToLower().Contains(model.FilterText.ToLower()));
            }

            if (model.SortBy == nameof(Common.Customer.Name))
            {
                query = model.SortDirection == Common.SortDirection.Ascending
                            ? query.OrderBy(c => c.Name)
                            : query.OrderByDescending(c => c.Name);
            }
            else if (model.SortBy == nameof(Common.Customer.State))
            {
                query = model.SortDirection == Common.SortDirection.Ascending
                            ? query.OrderBy(c => c.State)
                            : query.OrderByDescending(c => c.State);
            }
            else if (model.SortBy == nameof(Common.Customer.Gender))
            {
                query = model.SortDirection == Common.SortDirection.Ascending
                            ? query.OrderBy(c => c.Gender)
                            : query.OrderByDescending(c => c.Gender);
            }
            else if (model.SortBy == nameof(Common.Customer.Active))
            {
                query = model.SortDirection == Common.SortDirection.Ascending
                            ? query.OrderBy(c => c.Active)
                            : query.OrderByDescending(c => c.Active);
            }
            else
            {
                query = model.SortDirection == Common.SortDirection.Ascending
                            ? query.OrderBy(c => c.Name)
                            : query.OrderByDescending(c => c.Name);
            }

            Common.SearchResponse<Common.Customer> response = new Common.SearchResponse<Common.Customer>();
            response.Total = await query.CountAsync();

            var dataResponse = await query.Skip(model.Page * model.PageSize)
                                        .Take(model.PageSize)
                                        .ToListAsync();

            response.Results = dataResponse.Select(c => new Common.Customer()
            {
                Id = c.Id,
                Name = c.Name,
                City = c.City,
                State = c.State,
                Postal = c.Postal,
                Gender = c.Gender.Value,
                Active = c.Active.Value,
            }).ToList();

            return Ok(response);
        }



        [HttpGet]
        [Authorize]
        [Route("api/v1/seed/customers/{number}")]
        async public Task<IActionResult> SeedCustomers(int number)
        {
            string userId = User.GetUserId();

            for (int a = 0; a < number; a++)
            {
                var customer = new Database.Customer()
                {
                    Name = LoremNET.Lorem.Words(2),
                    Gender = (Common.Gender)LoremNET.Lorem.Number(0, 2),
                    Email = LoremNET.Lorem.Email(),
                    Phone = LoremNET.Lorem.Number(1111111111, 9999999999).ToString(),
                    Address = $"{LoremNET.Lorem.Number(100, 10000).ToString()} {LoremNET.Lorem.Words(1)}",
                    City = LoremNET.Lorem.Words(1),
                    State = LoremNET.Lorem.Words(1),
                    Postal = LoremNET.Lorem.Number(11111, 99999).ToString(),
                    BirthDate = LoremNET.Lorem.DateTime(1923, 1, 1),
                    Notes = LoremNET.Lorem.Paragraph(5, 10, 10),
                    Active = LoremNET.Lorem.Number(0, 1) == 0 ? false : true,
                    OwnerId = userId
                };

                _db.Customers.Add(customer);
                await _db.SaveChangesAsync();
            }

            return Ok();
        }









        [HttpGet]
        [Authorize]
        [Route("api/v1/customer/{customerId}/upload")]
        public async Task<IActionResult> FolderItemGenerateUploadSasUri(string customerId)
        {
            var userId = User.GetUserId();

            var result = await GetUploadSasForNewBlob();

            if(result != null)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPost]
        [Authorize]
        [Route("api/v1/customer/{customerId}/file")]
        public async Task<IActionResult> CustomerFileCreate(string customerId, [FromBody] Common.UploadCustomerFile model)
        {
            var userId = User.GetUserId();

            CustomerFile newFolderItem = new CustomerFile()
            {
                CustomerId = customerId,
                BlobId = model.BlobId,
                PrettyFileName = model.PrettyFileName,
                MimeType = model.MimeType,
                UploadedBy = userId,
                FileSize = model.FileSize
            };
            _db.CustomerFiles.Add(newFolderItem);
            await _db.SaveChangesAsync();

            return Ok(newFolderItem);
        }

        [HttpGet]
        [Authorize]
        [Route("api/v1/customer/{customerId}/file/{customerFileId}")]
        public async Task<IActionResult> CustomerFileGetById(string customerId, string customerFileId)
        {
            string userId = User.GetUserId();

            var customerFile = await _db.CustomerFiles
                        .Include(x => x.Customer)
                        .FirstOrDefaultAsync(o => o.CustomerId == customerId && o.Id == customerFileId);

            var sasUri = await GetViewSasForBlob(customerFile.BlobId);

            Common.CustomerFile response = new Common.CustomerFile()
            {
                Id = customerFile.Id,
                PrettyFileName = customerFile.PrettyFileName,
                FileSize = customerFile.FileSize,
                BlobId = customerFile.BlobId,
                ItemUri = sasUri.Sas,
                MimeType = customerFile.MimeType,
                UploadedBy = customerFile.UploadedBy,
                CreatedOn = customerFile.CreatedOn
            };

            return Ok(response);
        }

        [HttpPut]
        [Authorize]
        [Route("api/v1/customer/{customerId}/file/{customerFileId}")]
        public async Task<IActionResult> CustomerFileUpdateById([FromBody]Common.CustomerFile model, string customerId, string customerFileId)
        {
            string userId = User.GetUserId();

            var customerFile = await _db.CustomerFiles
                        .FirstOrDefaultAsync(o => o.CustomerId == customerId && o.Id == customerFileId);

            if(customerFile != null)
            {
                customerFile.PrettyFileName = model.PrettyFileName;
                await _db.SaveChangesAsync();
            }

            return Ok();
        }

        [HttpDelete]
        [Authorize]
        [Route("api/v1/customer/{customerId}/file/{customerFileId}")]
        public async Task<IActionResult> FolderItemDeleteById(string customerId, string customerFileId)
        {
            string userId = User.GetUserId();
            var customerFile = await _db.CustomerFiles
                        .FirstOrDefaultAsync(o => o.CustomerId == customerId && o.Id == customerFileId);

            if(customerFile != null)
            {
                await DeleteBlob(customerFile.BlobId);

                _db.CustomerFiles.Remove(customerFile);
                await _db.SaveChangesAsync();
            }

            return Ok();
        }

        [HttpPost]
        [Authorize]
        [Route("api/v1/customer/{customerId}/files")]
        async public Task<IActionResult> FolderItemsSearch([FromBody]Common.Search model, string customerId)
        {
            string userId = User.GetUserId();

            var query = _db.CustomerFiles
                        .Where(o => o.CustomerId == customerId);

            if (!string.IsNullOrEmpty(model.FilterText))
            {
                query = query.Where(x => x.PrettyFileName.Contains(model.FilterText));
            }

            if (model.SortBy == nameof(Common.CustomerFile.PrettyFileName))
            {
                query = model.SortDirection == Common.SortDirection.Ascending
                            ? query.OrderBy(c => c.PrettyFileName)
                            : query.OrderByDescending(c => c.PrettyFileName);
            }
            else if (model.SortBy == nameof(Common.CustomerFile.FileSize))
            {
                query = model.SortDirection == Common.SortDirection.Ascending
                            ? query.OrderBy(c => c.FileSize)
                            : query.OrderByDescending(c => c.FileSize);
            }
            else if (model.SortBy == nameof(Common.CustomerFile.CreatedOn))
            {
                query = model.SortDirection == Common.SortDirection.Ascending
                            ? query.OrderBy(c => c.CreatedOn)
                            : query.OrderByDescending(c => c.CreatedOn);
            }
            else
            {
                query = model.SortDirection == Common.SortDirection.Ascending
                            ? query.OrderBy(c => c.PrettyFileName)
                            : query.OrderByDescending(c => c.PrettyFileName);
            }

            Common.SearchResponse<Common.CustomerFile> response = new Common.SearchResponse<Common.CustomerFile>();
            response.Total = await query.CountAsync();

            var dataResponse = await query.Skip(model.Page * model.PageSize)
                                        .Take(model.PageSize)
                                        .ToListAsync();

            response.Results = dataResponse.Select(c => new Common.CustomerFile()
            {
                Id = c.Id,
                BlobId = c.BlobId,
                PrettyFileName = c.PrettyFileName,
                MimeType = c.MimeType,
                UploadedBy = c.UploadedBy,
                CreatedOn = c.CreatedOn,
                FileSize = c.FileSize
                
            }).ToList();

            return Ok(response);
        }







        async private Task<BlobContainerClient> GetContainer()
        {
            var container = new BlobContainerClient(_configuration.GetConnectionString("AzureStorageConnectionString"), _configuration["AzueStorageContainerName"]);
            var createResponse = await container.CreateIfNotExistsAsync();

            if (createResponse != null && createResponse.GetRawResponse().Status == 201)
                await container.SetAccessPolicyAsync(Azure.Storage.Blobs.Models.PublicAccessType.None);

            return container;
        }

        async private Task DeleteBlob(string blobId)
        {
            var container = await GetContainer();
            await container.DeleteBlobIfExistsAsync(blobId, Azure.Storage.Blobs.Models.DeleteSnapshotsOption.IncludeSnapshots);

        }

        async private Task<Common.SasUri> GetUploadSasForNewBlob()
        {
            var container = await GetContainer();

            var blobId = Guid.NewGuid().ToString();
            var blob = container.GetBlobClient(blobId); //model.FileName

            // If a blob with the same name exists, then we delete the Blob and its snapshots.
            await blob.DeleteIfExistsAsync(Azure.Storage.Blobs.Models.DeleteSnapshotsOption.IncludeSnapshots);

            BlobSasBuilder sasBuilder = new BlobSasBuilder();
            sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(10);
            sasBuilder.SetPermissions(BlobContainerSasPermissions.Write);

            var sasUri = blob.GenerateSasUri(sasBuilder);

            return new Common.SasUri()
            {
                Sas = sasUri,
                BlobId = blobId
            };
        }

        async private Task<Common.SasUri> GetViewSasForBlob(string blobId)
        {
            var container = await GetContainer();

            var blob = container.GetBlobClient(blobId); //model.FileName

            BlobSasBuilder sasBuilder = new BlobSasBuilder();
            sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(1);
            sasBuilder.SetPermissions(BlobContainerSasPermissions.Read);

            var sasUri = blob.GenerateSasUri(sasBuilder);

            return new Common.SasUri()
            {
                Sas = sasUri,
                BlobId = blobId
            };
        }
    }//End Controller
}


