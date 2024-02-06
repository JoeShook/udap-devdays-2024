# udap-devdays-2024
Spin up a local UDAP playground.  Includes FHIR server, static certificates server UDAP Auth Server and UDAP IDP Server.


[udap-dotnet](https://github.com/udap-tools/udap-dotnet) tutorial.

UDAP is the acronym for [Unified Data Access Profiles](https://www.udap.org/).
The HL7 "[Security IG](http://hl7.org/fhir/us/udap-security/)" is a constraint on UDAP.  The actual implementation guide has a long name of "Security for Scalable Registration, Authentication, and Authorization".

- FHIR® is the registered trademark of HL7 and is used with the permission of HL7. Use of the FHIR trademark does not constitute endorsement of the contents of this repository by HL7.
- UDAP® and the UDAP gear logo, ecosystem gears, and green lock designs are trademarks of UDAP.org. UDAP Draft Specifications are referenced and displayed in parts of this source code to document specification implementation.

In 2023 the [udap-devdays-2023](https://github.com/JoeShook/udap-devdays-2023) presentation contained detailed instructions on how to setup Objectives 1 through 3.  That presentation was very detailed concerning setup.
This years presentation will focus on Objective 4, Tiered OAuth and put less into the setup of the FHIR server and UDAP Auth Server details.  [udap-devdays-2023](https://github.com/JoeShook/udap-devdays-2023) is a good 
reference.   

## Objectives

1. 🧩 Host UDAP Metadata on a FHIR Server and secure access f
2. 🧩 Host UDAP Authorization Server and perform Dynamic Client Registration (DCR [RFC 7591](https://datatracker.ietf.org/doc/html/rfc7591))
3. 🧩 Secure the FHIR Server with UDAP
4. 🧩 Enabled Tiered OAuth and perform Dynamic Client Registration (DCR [RFC 7591](https://datatracker.ietf.org/doc/html/rfc7591)) with UDAP Auth Server actingas the client.



## Prerequisites

Clone the udap-dotnet repository.

````cli
git clone https://github.com/udap-tools/udap-dotnet.git
````

Ensure you can compile and run UdapEd.Server ahead of time.  It requires .NET 8.0.

We will run the UdapEd.Server project locally to test Discovery, DCR, Token Access and finally request a resource.  
Then we will let the user include the base URL of a preferred OpenID Connect Identity Provider (IdP).


## Things to look out for

If the developer regenerates certificates with the udap.pki.devdays project during the Tutorial delete the udap.authserver.devdays.EntityFramework.db database.  And restart udap.authserver.devdays.


### On Windows

If the developer regenerates certificates with the udap.pki.devdays during the Tutorial they may then need to launch mmc.exe, the Certificates snap-in for the current user.  Go to Intermediate Certification Authorities and delete the DevDaysSubCA_1.  


## Descriptions of each project

### udap.pki.devdays

This is a simple cli that generates all PKI needed for this desktop lab.  It generates a root CA, a sub CA, and a leaf UDAP certificate.  For the lab
three PKIs are generated.  On is RSA256, one is ECDSA nistP384 and the last is RSA256 where the UDAP client certificate has been revoked.  


### udap.certificates.server.devdays
This is a static certificate server, serving the certificate material that would typically be available for download.  One of those items is the intermediate certificates.
The other are the certificate revocation lists.  This allows you to test from your local machine without the gymnastics of setting up a real certificate server.

### udap.fhirserver.devdays

The FHIR server has one patient resource loaded.  This FHIR server is a simple DemoFileSystemFhirServer implementation of Brian Postlethwaite’s [fhir-net-web-api](https://github.com/brianpos/fhir-net-web-api/tree/feature/r4b). 



### udap.authserver.devdays


### udap.idp.server.devdays
