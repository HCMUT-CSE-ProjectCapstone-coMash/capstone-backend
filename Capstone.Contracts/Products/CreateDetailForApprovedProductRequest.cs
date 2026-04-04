namespace Capstone.Contracts.Products;

public record CreateDetailForApprovedProductRequest(
    List<ProductQuantity> Quantities
);