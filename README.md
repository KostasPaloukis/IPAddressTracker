# IP Address Tracker
## Overview
This project consists of a REST API written in .NET Core 6. The API allows users to retrieve information about IP addresses, update IP  addresses informations, and generate reports for one or more countries. Also intergates with IP2C, which is a free web service, in order to get these details.

## Installation
To install the IP Address Tracker, follow these steps:

1. Clone the repository to your local machine. 
2. Modify the appsettings.json file with your SQL Server connection string. 
3. Build the solution in Visual Studio. Run the TrackerIP.WebApi project.

## API Endpoints
This project includes the following API endpoints:

### IP Information Endpoint
This endpoint allows users to retrieve information about a specific IP address.

URL: /api/TrackerIP/details/{ip} 

Method: GET

Parameters: ip (required): The IP address to look up. 

Response: CountryName: The name of the country associated with the IP address. TwoLetterCode: The two-letter code for the country. ThreeLetterCode: The three-letter code for the country.

### Update IP Information
This endpoint updates IP information for all IPs in the database.

URL: /api/TrackerIP/update-all

Method: POST

Response: Status: Indicates whether the update was successful or not.

In addition there is a background service that execute the same functionallity every one hour.

### Report Endpoint
This endpoint generates a report of IP addresses by country.

URL: /api/TrackerIP/country-report

Method: GET 

Parameters: country_codes (optional): An array of two-letter country codes to include in the report. 

Response: CountryName: The name of the country. AddressesCount: The number of IP addresses associated with the country. LastAddressUpdated: The date and time that the last IP address associated with the country was updated.

## Technologies Used
* .NET Core 6
* REST API
* Entity Framework Core
* SQL Server
* Caching (MemoryCache)
* BackgroundService
* Unit Tests
