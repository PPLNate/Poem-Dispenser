# Poem-Dispenser
Written by Nate Broomfield for Providence Public Library

Poem Dispenser listens for a spacebar press, then picks a random text file from a specified directory and prints it.

Configurations should be saved in a textfile with the name poemzconfig.ppl in the same directory as the executable for this application. The format of the config file should be as follows:


            <poemdirectory>c:\poemfolder\
            <poemfileextension>*.txt
            <logdirectory>c:\poemfolder\
            <logfile>poemlog.log
            <linewidth>300
            <paperwidth>300
            <paperheight>5000
            <leftmargin>10
            <rightmargin>10
            <topmargin>10
            <bottommargin>10
            <font>Times
            <exitmessage>Message printed at the bottom
            <logofile>logofile.jpg
            <printlogo>true
            <printer>default

All measurements are in pixels.
