﻿using Configuration;
using DbContext;
using DbModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Models;
using Newtonsoft.Json;
using System.Data.Common;

namespace DbRepos
{
    public class TemplateRepository
    {
        private readonly string _dbLogin;
        private readonly ILogger<TemplateRepository> _logger;

        public TemplateRepository(ILogger<TemplateRepository> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            var dbLoginDetail = AppConfig.DbLoginDetails("DbLogins");
            _dbLogin = dbLoginDetail.DbConnectionString ?? throw new InvalidOperationException("Database connection string is not configured.");
        }

        public async Task<TicketHandling> CreateTemplateAsync(TemplateCUdto _src)
        {
            try
            {
                await using var db = TicketTemplateDbContext.Create(_dbLogin);

                var maxShowEventInfo = await db.TicketTemplate.MaxAsync(t => (int?)t.ShowEventInfo) ?? 0;
                _src.ShowEventInfo = maxShowEventInfo + 1;

                var newTicketTemplate = new TicketTemplateDbM(_src)
                {
                    ShowEventInfo = _src.ShowEventInfo
                };

                await db.TicketTemplate.AddAsync(newTicketTemplate);
                await db.SaveChangesAsync();

                _logger.LogInformation("New template created with ShowEventInfo {NewShowEventInfo}", newTicketTemplate.ShowEventInfo);
                return newTicketTemplate.TicketsHandling;
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database update error in CreateTemplateAsync.");
                throw new InvalidOperationException("Failed to create new template due to database update error.", dbEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateTemplateAsync: {ErrorMessage}", ex.Message);
                throw new InvalidOperationException("An unexpected error occurred while creating a new template.", ex);
            }
        }

        public async Task<ITicketTemplate> DeleteTemplateAsync(Guid id)
        {
            try
            {
                await using var db = TicketTemplateDbContext.Create(_dbLogin);
                var templateToDelete = await db.TicketTemplate.FindAsync(id);

                if (templateToDelete == null)
                {
                    _logger.LogWarning("Template with ID {Id} not found.", id);
                    throw new KeyNotFoundException($"Template with id {id} not found.");
                }

                db.TicketTemplate.Remove(templateToDelete);
                await db.SaveChangesAsync();

                _logger.LogInformation("Template with ID {Id} has been deleted.", id);
                return templateToDelete;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteTemplateAsync: {ErrorMessage}", ex.Message);
                throw new InvalidOperationException($"An error occurred while attempting to delete template with ID {id}.", ex);
            }
        }

        public async Task<byte[]> GetFileDataAsync(int? fileStorageID)
        {
            await using var db = TicketTemplateDbContext.Create(_dbLogin);

            try
            {
                var fileStorage = await db.FileStorage.FindAsync(fileStorageID);

                if (fileStorage == null)
                {
                    _logger.LogWarning("File with ID {FileStorageID} not found.", fileStorageID);
                    throw new KeyNotFoundException($"File with ID {fileStorageID} not found.");
                }

                return fileStorage.Data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving file data for FileStorageID {FileStorageID}. Details: {Message}", fileStorageID, ex.Message);
                throw new InvalidOperationException($"Failed to retrieve the file data for FileStorageID {fileStorageID}.", ex);
            }
        }

        public async Task<string> GetFilePathAsync(int fileId)
        {
            try
            {
                await using var db = TicketTemplateDbContext.Create(_dbLogin);

                var fileData = await db.FileStorage
                    .Where(f => f.FileStorageId == fileId)
                    .Join(db.FileDescription,
                          storage => storage.FileStorageId,
                          description => description.FileStorageId,
                          (storage, description) => new { storage.Data, description.Name })
                    .FirstOrDefaultAsync();

                if (fileData == null)
                {
                    _logger.LogError("File with ID {FileId} not found.", fileId);
                    throw new FileNotFoundException($"File with ID {fileId} not found.");
                }

                var tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + Path.GetExtension(fileData.Name));

                await File.WriteAllBytesAsync(tempFilePath, fileData.Data);

                _logger.LogInformation("Temporary file created at {TempFilePath} for file ID {FileId}.", tempFilePath, fileId);

                return tempFilePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GetFilePathAsync for file ID {FileId}.", fileId);
                throw new InvalidOperationException("An unexpected error occurred while retrieving file path.", ex);
            }
        }

        public async Task<TicketHandling?> GetPredefinedTicketHandlingAsync(int showEventInfo)
        {
            try
            {
                await using var db = TicketTemplateDbContext.Create(_dbLogin);
                var predefinedTemplate = await db.TicketTemplate
                                                 .AsNoTracking()
                                                 .FirstOrDefaultAsync(t => t.ShowEventInfo == showEventInfo);

                if (predefinedTemplate == null)
                {
                    _logger.LogWarning("No predefined TicketHandling found for ShowEventInfo: {ShowEventInfo}", showEventInfo);
                    return null;
                }

                return predefinedTemplate.TicketsHandling;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPredefinedTicketHandlingAsync for ShowEventInfo: {ShowEventInfo}. {ErrorMessage}", showEventInfo, ex.Message);
                throw new InvalidOperationException($"Failed to retrieve predefined TicketHandling for ShowEventInfo: {showEventInfo}.", ex);
            }
        }

        public async Task<TicketTemplateDto> GetTemplateByIdAsync(Guid ticketTemplateId)
        {
            await using var db = TicketTemplateDbContext.Create(_dbLogin);
            var template = await db.TicketTemplate
                .Where(t => t.TicketTemplateId == ticketTemplateId)
                .Select(t => new TicketTemplateDto
                {
                    TicketTemplateId = t.TicketTemplateId,
                    ShowEventInfo = t.ShowEventInfo,
                    TicketHandlingJson = t.TicketsHandlingJson,
                    Name = t.Name
                }).FirstOrDefaultAsync();

            return template ?? throw new KeyNotFoundException($"Template with ID {ticketTemplateId} not found.");
        }

        public async Task<TicketTemplateDbM?> GetTicketTemplateByShowEventInfoAsync(int showEventInfo)
        {
            try
            {
                await using var db = TicketTemplateDbContext.Create(_dbLogin);
                var template = await db.TicketTemplate
                                                 .AsNoTracking()
                                                 .FirstOrDefaultAsync(t => t.ShowEventInfo == showEventInfo);

                if (template == null)
                {
                    _logger.LogWarning("No Template found with ShowEventInfo: {ShowEventInfo}", showEventInfo);
                    return null;
                }

                return template;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetTicketTemplateByShowEventInfoAsync for ShowEventInfo: {ShowEventInfo}. {ErrorMessage}", showEventInfo, ex.Message);
                throw new InvalidOperationException($"Failed to retrieve Template with ShowEventInfo: {showEventInfo}.", ex);
            }
        }

        public async Task<IEnumerable<TicketsDataView>> GetTicketsDataByWebbUidAsync(Guid webbUid)
        {
            await using var db = TicketTemplateDbContext.Create(_dbLogin);

            var tickets = await db.Vy_ShowTickets
                                  .Where(ticket => ticket.WebbUid == webbUid)
                                  .ToListAsync();

            _logger.LogInformation("{TicketsCount} tickets retrieved for WebbUid: {WebbUid}", tickets.Count, webbUid);
            return tickets;
        }

        public async Task<List<TicketTemplateDto>> ReadTemplatesAsync()
        {
            try
            {
                await using var db = TicketTemplateDbContext.Create(_dbLogin);
                var templates = await db.TicketTemplate.Select(t => new TicketTemplateDto
                {
                    TicketTemplateId = t.TicketTemplateId,
                    ShowEventInfo = t.ShowEventInfo,
                    TicketHandlingJson = t.TicketsHandlingJson,
                    Name = t.Name
                }).ToListAsync();

                _logger.LogInformation("Retrieved {TemplateCount} templates", templates.Count);
                return templates;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ReadTemplatesAsync: {ErrorMessage}", ex.Message);
                throw new InvalidOperationException("An error occurred while reading templates.", ex);
            }
        }

        public async Task<int> SaveFileToDatabaseAsync(byte[] fileData, string description, string name)
        {
            if (fileData == null || fileData.Length == 0)
            {
                _logger.LogError("Attempted to save an empty or null file data.");
                throw new ArgumentException("The file data cannot be empty or null.");
            }

            string? fileExtension = Path.GetExtension(name)?.ToLowerInvariant();

            int fileTypeId = fileExtension switch
            {
                ".jpg" => 1,
                ".png" => 2,
                _ => throw new ArgumentException("Only JPG and PNG files are accepted.")
            };

            int fileSize = fileData.Length;
            int fileStorageId = 0;

            await using var db = TicketTemplateDbContext.Create(_dbLogin);

            var executionStrategy = db.Database.CreateExecutionStrategy();

            await executionStrategy.ExecuteAsync(async () =>
            {
                await using var transaction = await db.Database.BeginTransactionAsync();

                try
                {
                    var existingFileDescription = await db.FileDescription
            .FirstOrDefaultAsync(fd => fd.FileSize == fileSize && fd.FileTypeId == fileTypeId);

                    if (existingFileDescription != null)
                    {
                        fileStorageId = existingFileDescription.FileStorageId;
                        _logger.LogInformation("Using existing FileStorage entry with ID {Id}", fileStorageId);
                    }
                    else
                    {
                        var fileStorage = new FileStorage
                        {
                            Data = fileData,
                        };

                        await db.FileStorage.AddAsync(fileStorage);
                        await db.SaveChangesAsync();

                        var fileDescription = new FileDescription
                        {
                            FileStorageId = fileStorage.FileStorageId,
                            FileTypeId = fileTypeId,
                            FileCategoryId = 3,
                            Description = description,
                            Name = name,
                            CreateTime = DateTime.Now,
                            FileSize = fileData.Length
                        };

                        await db.FileDescription.AddAsync(fileDescription);
                        await db.SaveChangesAsync();

                        fileStorageId = fileStorage.FileStorageId;
                    }

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Failed to save file to database.");
                    throw new InvalidOperationException("Failed to save the file to database.", ex);
                }
            });

            return fileStorageId;
        }

        public async Task<TicketTemplateDto> UpdateTemplateAsync(TicketTemplateDto templateDto)
        {
            await using var db = TicketTemplateDbContext.Create(_dbLogin);
            var templateToUpdate = await db.TicketTemplate.FindAsync(templateDto.TicketTemplateId) ?? throw new KeyNotFoundException($"Template with ID {templateDto.TicketTemplateId} not found.");
            if (templateDto.Name != null)
            {
                templateToUpdate.Name = templateDto.Name;
            }
            else
            {
                _logger.LogWarning("Attempted to update a template with a null name for template ID {Id}.", templateDto.TicketTemplateId);
                templateToUpdate.Name = "Default Name";
            }
            if (!string.IsNullOrWhiteSpace(templateDto.TicketHandlingJson))
            {
                var ticketHandlingFromDto = JsonConvert.DeserializeObject<TicketHandling>(templateDto.TicketHandlingJson);
                if (ticketHandlingFromDto != null)
                {
                    templateToUpdate.TicketsHandling = ticketHandlingFromDto;
                }
            }

            await db.SaveChangesAsync();

            _logger.LogInformation("Template with ID {Id} has been updated.", templateDto.TicketTemplateId);
            return templateDto;
        }
    }
}