# FileFinder
## About The Project
This is a desktop application designed with purpose of finding files on device's disks using synchronous and parallel methods. 

### Built With

* WPF
* C# 11
* [Mvvm Cross](https://www.mvvmcross.com/)


## Getting Started

This is an example of how you may give instructions on setting up your project locally.
To get a local copy up and running follow these simple example steps.

### Prerequisites

This is an example of how to list things you need to use the software and how to install them.
* [Download & install .NET 7 runtime](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)


## Usage

Application receives following input from user:
* Directory - full directory path that will be scanned in search for file (along with it's subdirectories that can be accessed)
* File - file name which can be one of the following: full file name (with extension), part of name, file extension
* Search mode - radio buttons that control invoking method of classes that implement ISearchEngine interface:
    * Main thread - UI will freeze and application will search for files synchronously with recursive method
    * Separate thread - as in Main thread mode, but there is possibility to stop search and application constantly reports its search progress (with progress bar and already found files) 
    * Parallel - asynchronous mode, that will perform selected with slider by user X operations in parallel using available threads, along with reports implemented in Separate thread mode

 After finishing search application writes count of checked directories and time it took to complete search operation in the console located on the right side.
 If user decides to stop search, new parameters can be selected to start it anew.