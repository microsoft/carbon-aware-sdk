#!/bin/bash
set -x

#####################################################################################
# THIS SCRIPT ASSUMES YOU HAVE THE FOLLOWING REMOTES SETUP.
# DO NOT RUN THIS UNTIL YOU DO (either HTTPS or SSH remotes will work)
#####################################################################################
#
# git remote -v
# gsf     https://github.com/Green-Software-Foundation/carbon-aware-sdk.git (fetch)
# gsf     https://github.com/Green-Software-Foundation/carbon-aware-sdk.git (push)
# origin  https://github.com/microsoft/carbon-aware-sdk.git (fetch)
# origin  https://github.com/microsoft/carbon-aware-sdk.git (push)
#####################################################################################

# Remove the local release branch if exists
git branch -D release

# Get the latest commits
git fetch gsf
git fetch origin

# checkout gsf/dev and cut a new release branch from it
git checkout gsf/dev
git checkout -b release

# Cherry-pick our required, but unmerged commits onto the branch
git cherry-pick 4d4a2ef6172754899e9040b7f9e3cf272989e0b3 # issue #161 - new data source interfaces
git cherry-pick 357b52b262f01bbad45332173eab91b0ef4e7620 # issue #161 - data source interfaces in config
git cherry-pick 6408d2928224bc68d5167b429c502fa7cac560da # issue #160 - C# library
git cherry-pick b814a2d1e0c23caeb388ec71ffc6e39ab8f69f76 # issue #166 - SDK library tooling
git cherry-pick d47f7da5ccba45f1bbf22f0dbd0daa1c02346e91 # issue #164 - New DataSource Config Schema

###### Adding features to this script:
###### 1) switch to your feature branch
# git switch <###/your-feature-branch>
######
###### 2) If your feature branch is not based off of `release`
######    SKIP to step 4
######
###### 3) Your feature branch is based off of `release`,
###### so make sure it is up-to-date with the latest `release` commits
# git pull --rebase origin release
#######
####### 3a) Resolve any conflicts.
#######
####### 4) Squash your feature into a single commit
# git rebase -i HEAD~<number-of-commits-in-your-feature>  # EG: git rebase -i HEAD~3
#######
####### In the interactive window, squash your those commits into a single commit
####### follows the naming convention:
####### [M#][Issue#] Feature name
#######
####### 5) Now that you have a single commit for your feature,
####### copy the commit, and add it to the release branch
# git switch release
# git cherry-pick <your-single-feature-commit>
#######
####### Resolve any conflicts.
####### 
####### 6) Get your final commit hash and add it to this file
####### NOTE: it will be different from the commit you cherry-picked
####### if you had to resolve any conflicts.
####### It should follow the format of the others
####### git cherry-pick <HASH> # issue ### - <Short description>