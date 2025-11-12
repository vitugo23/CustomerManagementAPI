using Xunit;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;
using CustomerManagement.Models.DTOs;

namespace CustomerManagement.Tests.DTOs
{
    public class DtoValidationTests
    {
        [Fact]
        public void CreateCustomerDto_WithValidData_PassesValidation()
        {
            // Arrange
            var dto = new CreateCustomerDto
            {
                FirstName = "Valid",
                LastName = "Customer",
                Email = "valid@test.com",
                Phone = "555-123-4567",
                CustomerType = "Individual"
            };

            // Act
            var validationResults = ValidateModel(dto);

            // Assert
            validationResults.Should().BeEmpty();
        }

        [Fact]
        public void CreateCustomerDto_WithInvalidEmail_FailsValidation()
        {
            // Arrange
            var dto = new CreateCustomerDto
            {
                FirstName = "Invalid",
                LastName = "Email",
                Email = "not-an-email",
                Phone = "555-123-4567"
            };

            // Act
            var validationResults = ValidateModel(dto);

            // Assert
            validationResults.Should().NotBeEmpty();
            validationResults.Should().Contain(r => r.MemberNames.Contains("Email"));
        }

        [Theory]
        [InlineData("", "Valid", false)] // Empty first name
        [InlineData("Valid", "", false)] // Empty last name
        [InlineData("Valid", "Customer", true)] // Valid both
        public void CreateCustomerDto_NameValidation_WorksCorrectly(string firstName, string lastName, bool shouldBeValid)
        {
            // Arrange
            var dto = new CreateCustomerDto
            {
                FirstName = firstName,
                LastName = lastName,
                Email = "test@example.com",
                Phone = "555-123-4567"
            };

            // Act
            var validationResults = ValidateModel(dto);

            // Assert
            if (shouldBeValid)
            {
                validationResults.Should().BeEmpty();
            }
            else
            {
                validationResults.Should().NotBeEmpty();
            }
        }

        private static IList<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(model);
            Validator.TryValidateObject(model, validationContext, validationResults, true);
            return validationResults;
        }
    }
}