<!-- Improved compatibility of back to top link: See: https://github.com/elliot9802/PdfTemplateCreater/pull/73 -->
<a name="readme-top"></a>
<!--
*** Thanks for checking out the PdfTemplateCreater. If you have a suggestion
*** that would make this better, please fork the repo and create a pull request
*** or simply open an issue with the tag "enhancement".
*** Don't forget to give the project a star!
*** Thanks again! Now go create something AMAZING! :D
-->



<!-- PROJECT SHIELDS -->
<!--
*** I'm using markdown "reference style" links for readability.
*** Reference links are enclosed in brackets [ ] instead of parentheses ( ).
*** See the bottom of this document for the declaration of the reference variables
*** for contributors-url, forks-url, etc. This is an optional, concise syntax you may use.
*** https://www.markdownguide.org/basic-syntax/#reference-style-links
-->
[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![MIT License][license-shield]][license-url]
[![LinkedIn][linkedin-shield]][linkedin-url]




<br />
<div align="center">
  <!-- PROJECT LOGO 
  <a href="https://github.com/elliot9802/PdfTemplateCreater">
    <img src="https://github.com/elliot9802/PdfTemplateCreater/blob/master/PdfTemplateDesigner.png" alt="Logo" width="80" height="80">
  </a>
-->
  <h3 align="center">PdfTemplateCreater</h3>

  <p align="center">
    An awesome PDF template editor/creator!
    <br />
    <a href="https://github.com/elliot9802/PdfTemplateCreater"><strong>Explore the docs »</strong></a>
    <br />
    <br />
    <a href="https://github.com/elliot9802/PdfTemplateCreater">View Demo</a>
    ·
    <a href="https://github.com/elliot9802/PdfTemplateCreater/issues/new?labels=bug&template=bug-report---.md">Report Bug</a>
    ·
    <a href="https://github.com/elliot9802/PdfTemplateCreater/issues/new?labels=enhancement&template=feature-request---.md">Request Feature</a>
  </p>
</div>



<!-- TABLE OF CONTENTS -->
<details>
  <summary>Table of Contents</summary>
  <ol>
    <li>
      <a href="#about-the-project">About The Project</a>
      <ul>
        <li><a href="#built-with">Built With</a></li>
      </ul>
    </li>
    <li>
      <a href="#getting-started">Getting Started</a>
      <ul>
        <li><a href="#prerequisites">Prerequisites</a></li>
        <li><a href="#installation">Installation</a></li>
      </ul>
    </li>
    <!--<li><a href="#usage">Usage</a></li>-->
    <li><a href="#roadmap">Roadmap</a></li>
    <li><a href="#contributing">Contributing</a></li>
    <li><a href="#license">License</a></li>
    <li><a href="#contact">Contact</a></li>
   <!-- <li><a href="#acknowledgments">Acknowledgments</a></li>-->
  </ol>
</details>



<!-- ABOUT THE PROJECT -->
## About The Project
<a href="https://github.com/elliot9802/PdfTemplateCreater">
    <img src="https://github.com/elliot9802/PdfTemplateCreater/blob/master/PdfTemplateDesigner.png" alt="Logo" width="950" height="550">
</a>

A flexible API backend and Blazor frontend enabling users to generate custom PDF templates with selectable features for their articles. Supports dynamic backgrounds, customizable text, and choice of barcode or QR code, tailored to fit various article designs and business requirements.

Let's the user:
* Create a completely customizable PDF template.
* Use and edit existing templates.
* Simulate purchase process for customer with template of choice, endpoint that will also be used to create all the articles the user has bought.

<p align="right">(<a href="#readme-top">back to top</a>)</p>



### Built With

* [![C#][C#]][C#-url]
* [![.Net][.Net]][.Net-url]
* [![Blazor][Blazor]][Blazor-url]
* [![SQL Server][SQL Server]][SQLServer-url]
* [![Swagger][Swagger]][Swagger-url]
* [![Bootstrap][Bootstrap.com]][Bootstrap-url]

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- GETTING STARTED -->
## Getting Started

This section guides you through the steps to set up the project locally. Follow these instructions to get your local environment ready for development.

### Prerequisites
Before installing this project, ensure you have the necessary software installed. Here's what you'll need:

* .NET 6.0 SDK or later - You will need the .NET SDK to run the .NET commands and build the project. You can download it from [Microsoft's official site](https://dotnet.microsoft.com/en-us/download).
  ```sh
  dotnet --version #Check your .NET version
  ```
* Visual Studio 2022 or later (optional but recommended for development) - Visual Studio provides a powerful development environment for building .NET applications. Download it from [here](https://visualstudio.microsoft.com/downloads/).
* SQL Server - As this project uses Entity Framework with SQL Server, ensure you have SQL Server installed or have access to a SQL Server instance. Download SQL Server from [Microsoft's official site](https://www.microsoft.com/en-us/sql-server/sql-server-downloads).

### Installation

_Follow these steps to set up your project:_

1. Clone the Repository - Begin by cloning the repository to your local machine:
   ```sh
   git clone https://github.com/elliot9802/PdfTemplateCreater.git
   ```
2. Restore NuGet packages - Navigate to the project directory and restore the NuGet packages:
   ```sh
   cd PdfTemplateCreater
   dotnet restore
   ```
3. Set Up the Database - Make sure your SQL Server is running. Use Entity Framework migrations to set up your database:
   ```sh
   dotnet ef database update
   ```
4. Configure Application Settings - Adjust the application settings in `appsettings.json` or `appsettings.Development.json` as needed, particularly for database connections and other integrations like API keys.
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=your_server;Database=your_database;Trusted_Connection=True;"
   }
   ```
5. Start the Application - You can run the application directly using the .NET CLI or through Visual Studio:
   ```sh
   dotnet run
   ```
   If you are using Visual Studio, you can press `F5` or click on `Start Debugging`.

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- USAGE EXAMPLES
## Usage

Use this space to show useful examples of how a project can be used. Additional screenshots, code examples and demos work well in this space. You may also link to more resources.

_For more examples, please refer to the [Documentation](https://example.com)_

<p align="right">(<a href="#readme-top">back to top</a>)</p>
-->


<!-- ROADMAP -->
## Roadmap

- [x] Add Changelog
- [x] Add back to top links
- [ ] Add Additional Templates w/ Examples
- [ ] Multi-language Support
    - [x] Swedish
    - [ ] English

See the [open issues](https://github.com/elliot9802/PdfTemplateCreater/issues) for a full list of proposed features (and known issues).

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- CONTRIBUTING -->
## Contributing

Contributions are what make the open source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

If you have a suggestion that would make this better, please fork the repo and create a pull request. You can also simply open an issue with the tag "enhancement".
Don't forget to give the project a star! Thanks again!

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- LICENSE -->
## License

Distributed under the MIT License. See `LICENSE.txt` for more information.

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- CONTACT -->
## Contact

Elliot Segerlind - elliot@segerlind.se

Project Link: [https://github.com/elliot9802/PdfTemplateCreater](https://github.com/elliot9802/PdfTemplateCreater)

<p align="right">(<a href="#readme-top">back to top</a>)</p>


<!-- ACKNOWLEDGMENTS 
## Acknowledgments

Use this space to list resources you find helpful and would like to give credit to. I've included a few of my favorites to kick things off!

* [Choose an Open Source License](https://choosealicense.com)
* [GitHub Emoji Cheat Sheet](https://www.webpagefx.com/tools/emoji-cheat-sheet)
* [Malven's Flexbox Cheatsheet](https://flexbox.malven.co/)
* [Malven's Grid Cheatsheet](https://grid.malven.co/)
* [Img Shields](https://shields.io)
* [GitHub Pages](https://pages.github.com)
* [Font Awesome](https://fontawesome.com)
* [React Icons](https://react-icons.github.io/react-icons/search)

<p align="right">(<a href="#readme-top">back to top</a>)</p>
-->


<!-- MARKDOWN LINKS & IMAGES -->
<!-- https://www.markdownguide.org/basic-syntax/#reference-style-links -->
[contributors-shield]: https://img.shields.io/github/contributors/elliot9802/PdfTemplateCreater.svg?style=for-the-badge
[contributors-url]: https://github.com/elliot9802/PdfTemplateCreater/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/elliot9802/PdfTemplateCreater.svg?style=for-the-badge
[forks-url]: https://github.com/elliot9802/PdfTemplateCreater/network/members
[stars-shield]: https://img.shields.io/github/stars/elliot9802/PdfTemplateCreater.svg?style=for-the-badge
[stars-url]: https://github.com/elliot9802/PdfTemplateCreater/stargazers
[issues-shield]: https://img.shields.io/github/issues/elliot9802/PdfTemplateCreater.svg?style=for-the-badge
[issues-url]: https://github.com/elliot9802/PdfTemplateCreater/issues
[license-shield]: https://img.shields.io/github/license/elliot9802/PdfTemplateCreater.svg?style=for-the-badge
[license-url]: https://github.com/elliot9802/PdfTemplateCreater/blob/master/LICENSE.txt
[linkedin-shield]: https://img.shields.io/badge/-LinkedIn-black.svg?style=for-the-badge&logo=linkedin&colorB=555
[linkedin-url]: https://www.linkedin.com/in/elliot-segerlind-8085568b/
[product-screenshot]: images/screenshot.png
[.Net]: https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white
[.Net-url]: https://learn.microsoft.com/sv-se/dotnet/welcome
[Blazor]: https://img.shields.io/badge/blazor-%235C2D91.svg?style=for-the-badge&logo=blazor&logoColor=white
[Blazor-url]: https://reactjs.org/
[Vue.js]: https://img.shields.io/badge/Vue.js-35495E?style=for-the-badge&logo=vuedotjs&logoColor=4FC08D
[Vue-url]: https://vuejs.org/
[C#]: https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=csharp&logoColor=white
[C#-url]: https://svelte.dev/
[SQL Server]: https://img.shields.io/badge/Microsoft%20SQL%20Server-CC2927?style=for-the-badge&logo=microsoft%20sql%20server&logoColor=white
[SQLServer-url]: https://www.microsoft.com/en-us/sql-server/sql-server-downloads
[Bootstrap.com]: https://img.shields.io/badge/Bootstrap-563D7C?style=for-the-badge&logo=bootstrap&logoColor=white
[Bootstrap-url]: https://getbootstrap.com
[Swagger]: https://img.shields.io/badge/-Swagger-%23Clojure?style=for-the-badge&logo=swagger&logoColor=white
[Swagger-url]: https://swagger.io/
