# Customer Management System

## Overview
For Project 1, I decided to build a Customer Management System built with ASP.NET Core Minimal API, designed for businesses to manage customer records, addresses, and orders efficiently.

## Features
- **Full CRUD Operations** for customers
- **Address Management** (multiple addresses per customer)
- **Order Tracking** with many-to-many relationships
- **Advanced Search** by name, email, or customer type
- **RESTful API** with Swagger documentation
- **Data Persistence** with SQL Server and Entity Framework Core

## Database Schema
The system uses four main entities with two main relationships:
- **Customer → Address** (One-to-Many)
- **Customer ↔ Order** (Many-to-Many via CustomerOrder)

### Entities:
- `Customer`: Core customer information
- `Address`: Customer addresses (home, work, billing)
- `Order`: Order information and status
- `CustomerOrder`: Junction table for customer-order relationships

### Prerequisites
- .NET 9.0 SDK
- Docker Desktop
- SQL Azure Data Studio 
