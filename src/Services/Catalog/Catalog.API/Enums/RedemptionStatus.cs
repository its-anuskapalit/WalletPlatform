namespace Catalog.API.Enums;

public enum RedemptionStatus
{
    Pending    = 0,  // Created, points not yet deducted
    Processing = 1,  // Points deduction in progress
    Completed  = 2,  // Points deducted, item delivered
    Failed     = 3,  // Points deduction failed
    Cancelled  = 4   // Cancelled before processing
}