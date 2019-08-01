#!/bin/bash
echo 'Pulling GlitchedEpistle.Client.Windows'
git pull
echo 'Pulling shared codebase GlitchedEpistle.Client'
cd ../GlitchedEpistle.Client && git pull
