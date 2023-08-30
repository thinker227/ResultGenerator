The [generator](./SourceGenerator.cs) works in two steps:

1. Using `ForAttributeWithMetadataName`, visit all methods marked with `[ReturnsResult]` and create a [`ResultType`](./Models/ResultType.cs) model from the method, representing a result type to be generated.

2. For each generated model, generate the source for the result type using [`TextWriter`](./TextWriter.cs) and output it.

For every location in the models where a condition is followed by `return null;`, a corresponding diagnostic and analysis should exist in [Analysis](../Analysis), and there should be an attached comment pointing to that diagnostic. The [analyzer](../Analysis/Analyzer.cs) is used to report diagnostics about invalid code as opposed to the generator itself. This is mainly because an analyzer provides more flexibility than a generator, and generators producing diagnostics have been known to be buggy.
