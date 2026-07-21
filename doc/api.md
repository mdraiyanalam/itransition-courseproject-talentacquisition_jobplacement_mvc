\# API Documentation



\## Overview

This project primarily uses \*\*MVC Pattern\*\* with Razor Pages. There are no public REST APIs, but internal controller actions are documented below.



\### Main Endpoints



\#### Positions

\- `GET /Positions` → List all positions

\- `GET /Positions/Details/{id}` → Position details + discussions

\- `POST /Positions/Create` → Create new position (Recruiter/Admin)



\#### CVs

\- `GET /CVs/Create?positionId={id}` → Apply to position

\- `POST /CVs/Create` → Submit CV

\- `GET /CVs/Download/{id}` → Download PDF



\#### Admin

\- `GET /Admin/Users` → User management

\- `POST /Admin/DeleteUserConfirmed` → Delete user



\#### Profiles

\- `GET /Profiles` → Candidate profile

\- `POST /Profiles` → Update profile



\*\*Authentication\*\*

\- All sensitive actions require authentication + role authorization.

