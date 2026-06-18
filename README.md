# Recipe Management System

An ASP.NET Core MVC web application for creating, browsing, organizing, and reviewing recipes. Users can register, log in, save favorites, filter recipes by category, and manage their own recipe collection.

## Team

- Michele Argenti
- Silvia Castagnino
- Francesco Foresta

## Overview

Recipe Management System is built with ASP.NET Core 9, Entity Framework Core, ASP.NET Identity, and PostgreSQL. The application includes seeded sample data, category-based organization, role-based access, and a responsive Bootstrap-based UI.

## Features

- User registration, login, and logout with ASP.NET Identity.
- Role-based access for `Admin` and `User`.
- Browse all recipes or filter by category.
- View detailed recipe pages with ingredients, instructions, timing, servings, and categories.
- Create, edit, and delete recipes.
- Assign one or more categories to each recipe.
- Save and remove recipes from favorites.
- Search within the user's own recipes.
- Leave and edit reviews with ratings.
- View personal recipe and favorite collections.
- Seeded starter content for quick testing.
- Health check endpoint at `/health`.
- PWA-friendly layout and mobile-responsive navigation.

## Technology Stack

- ASP.NET Core MVC
- Razor Views
- Entity Framework Core
- PostgreSQL
- ASP.NET Identity
- Bootstrap 5
- Font Awesome

## Project Structure

- `Controllers/` - MVC controllers, including the recipes workflow.
- `Data/` - database context and seeding logic.
- `Models/` - domain entities and view models.
- `Views/` - Razor pages for home, recipes, shared layout, and validation.
- `Areas/Identity/` - ASP.NET Identity pages.
- `wwwroot/` - static assets, CSS, scripts, icons, and library files.

## Prerequisites

- .NET SDK 9.0
- PostgreSQL
- A code editor such as Visual Studio or VS Code

## Setup

1. Clone the repository.
2. Restore packages:

```bash
dotnet restore
```

3. Configure the database connection string.

The app expects `ConnectionStrings:DefaultConnection`. You can set it in one of these places:

- `appsettings.json`
- `appsettings.Development.json`
- User Secrets
- Environment variables

Example connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=recipe_management;Username=postgres;Password=your_password"
  }
}
```

4. Run the application:

```bash
dotnet run
```

5. Open the app in the browser using the URL shown in the terminal.

## Seeded Demo Accounts

The database initializer creates sample roles, users, categories, and recipes on first run.

## Core Pages

- Home page with featured stats and recent recipes.
- Recipe index with category filtering.
- Recipe details with reviews and favorite actions.
- Create/Edit/Delete recipe forms.
- My Recipes page for the signed-in user.
- Favorites page for saved recipes.
- Search My Recipes page.

## Error Handling

The application uses validation summaries and field-level validation in create/edit forms, authorization checks for protected actions, and a shared error page for unexpected failures.

## Deployment

The repository includes deployment support files for Railway and container-based hosting:

- `Dockerfile`
- `railway.json`

For deployment, set the production connection string and ensure the app can reach PostgreSQL in the target environment.

## Health Check

The app exposes a health check endpoint:

```text
/health
```

## Notes

- The app uses `EnsureCreatedAsync()` during startup to initialize the database.
- Sample categories include Breakfast, Lunch, Dinner, Dessert, Vegetarian, Vegan, and more.
- The UI is built to be responsive and PWA-friendly.

## License

This project includes the following license files:

- `LICENSE`
- `LICENSE-CODE`
