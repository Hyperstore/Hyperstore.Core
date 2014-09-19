**Hyperstore is an in-memory Model Oriented Database written in C# and fully extensible.**

It implements many concepts from Domain Driven Design and modeling framework and provides many features such as:

* In-memory transactional database using hypergraph to store elements and theirs properties,
* Thread-safe. Concurrent threads can manipulate the database with (ACID : Atomicity, Coherence, Isolation and Durability with adapter) transaction 
* Extensible meta-model describing all the elements of a domain along with validation rules,
* In-memory multiple domains hosting,
* Domain Driven Design implementation with automatic event notification,
* Persistence adapters,
* Event bus mechanism to collaborate between hyperstore instances,
* WPF binding including data error notification and calculated property dependencies, 
* Textual domain language definition,
* Undo/redo manager,
* Event bus ready


## How to use it

Hyperstore is available using nuget as a PCL library (Use hyperstore to find the corresponding package)

A domain must be describe before using it. There are several ways to do that :

* Code first : Directly by code with schema api
* Using the DSL Tool designer (available as a nuget package).
* Using a textual DSL using a specific Visual Studio editor package with syntax highlight, coloring and automatic code generation.

A domain description includes metadata definition for all entities, relationships and theirs properties, validation rules, commands and events.

To use a domain, load the schema definition and create an new domain instance in the main store (Hyperstore).

Then when you create a new entity or relationship, the domain will raise an event for each operations performed.

Consult samples and [wiki](https://github.com/Hyperstore/Hyperstore.Core/wiki) for documentation