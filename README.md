# FileSort

Sorting algorithm for large files.
File encoding is UTF-8.
Each line has format: `Number. String`
Sample of input file:

<pre>415. Apple
30432. Something something something
1. Apple
32. Cherry is the best
2. Banana is yellow</pre>

After processing we should get this:

<pre>
1. Apple
415. Apple
2. Banana is yellow
32. Cherry is the best
30432. Something something something
</pre>

Sorting should be done in that way: first order by String part, then sort by Number part.
Number does not have any range limits, so we will expect it to be `BigInteger`.

Since file size which we expect to be is ~100GB I suppose that variety of strings is really big and we can find there any combinations of two first letters across whole file.
That's the core assumption we will rely on to speedup process.

If file is smaller than 1GB:

<pre>Read file line by line and split it to different folders. 
Each folder is byte code of first character in the String part of the line.
So if it's "space" then it's - 32. Also all folder names have 3 letters. 
If byte code is less tan 100 then extra zeroes are added at left: "032".
</pre>

If file is bigger than 1GB:

<pre>Read file line by line and split it to two-level folders structore. 
It will be done in the same way, the only difference is that we will use second char to create nested folder.
</pre>

Then:

<pre>
Take first file after split.
Split it into smaller files with N lines.

That setting "LinesReadLimit" can be applied in the app.config.
Default value is 300_000 lines. If file exceeds that number of lines we then split it to files with that number of lines.

Sort all files.
Merge files: Take two first files, read them line by line, write to the third temp file.



Repeat action until we have only one final sorted file.

Move to the next file until all files are sorted.

When all files are sorted just combine them appropriately to the order of folders.
</pre>

Please note that algorithm does not have possibility to detect available RAM and adapt to it.
So, settings like "LinesReadLimit" and "LinesListsLimit" can be set manually.

During sorting of the file with size of 43GB (each line is ~500 characters) application used ~3GB of memory.
To decrease memory usage we can set LinesReadLimit to 100_000 and LinesListsLimit to 2.

To run file sort applicaiton you need to fill in `File` parameter in the app.config with the path to the input file.

<b>Please note</b>: Console output is redirected to the logs.txt file in the root folder of application.

# FileGenerator

Please note that file generator has such list of configurable parameters in app.config:

chars: set of characters which are used for generation `String` part of the line
size: desired file size in bytes
duplicatesProbability: percentage of possible lines duplicates in the file with the same `String` part and different `Number` part.
numberMinValue and numberMaxValue: range of generated numbers for `Number` part
outFile: file name of generated file
textMinLength and textMaxLength: range of length limit for `String` part.
