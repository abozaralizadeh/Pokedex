

Pokedx is an open source asp.net web app.

## Download and install .NET SDK

To start building .NET apps, download and install the .NET SDK (Software Development Kit).
Check everything installed correctly
Once you've installed, open a new command prompt and run the following command:
```sh
> dotnet
```
If the installation succeeded, you should see an output similar to the following:
```sh
Usage: dotnet [options]
Usage: dotnet [path-to-application]

Options:
-h|--help         Display help.
--info            Display .NET information.
--list-sdks       Display the installed SDKs.
--list-runtimes   Display the installed runtimes.

path-to-application:
The path to an application .dll file to execute.
```

## Run Pokedex using command line

In your command line, run the following commands:

```sh
> cd Pokedex\Pokedex
> dotnet run
```
 then open your browser and check the swagger link at http://localhost:5000/swagger/index.html

there you go!

## Run Pokedex using Docker

build the image

```sh
> cd Pokedex\Pokedex
> docker build . -t pokedex
```
and then run it

```sh
> docker run -d -p 8080:80 pokedex
```

## In Production

Remember to use a cache mechanism!
