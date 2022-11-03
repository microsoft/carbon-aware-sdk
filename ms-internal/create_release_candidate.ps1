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
git cherry-pick eb88e18d5e447deccc93d9e912148a8c12114cdb # issue 591 - location source
git cherry-pick b9d3728fc4b8829087f3108dd80b1d66b2e08653 # issue 583 - CLI
git cherry-pick 9d72a55aa1b2162c5b0347e428ed9a786301aca9 # issue #161 - new data source interfaces
git cherry-pick 1d3e8345ef91ec6061b5c1a9a6045f7490b0f269 # issue #161 - data source interfaces in config
git cherry-pick 8cfbb9cc2be33d552f92451d55f6cb53e45afbb0 # issue #160 - C# library
git cherry-pick f8a772d498e585b67aae310baaf5387c3faa9c0f # issue #166 - SDK library tooling

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