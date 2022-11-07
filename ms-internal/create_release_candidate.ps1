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
git cherry-pick 2765770a3749d31e0f80a3dec65c796457b5b588 # issue 591 - location source
git cherry-pick a58325ce071163050f69d9ce14eb3c71fed068f6 # issue 583 - CLI
git cherry-pick c1e41af726297b494337007d038ee857e434b21b # issue #161 - new data source interfaces
git cherry-pick ac91c74f7ca042c40271dd1d0258ce646f571b13 # issue #161 - data source interfaces in config
git cherry-pick 36237cae538ea519201667e0eefe3f876985de91 # issue #160 - C# library
git cherry-pick 12ac0f67d1ccc5eff7dfcd08f26d74463ab1ff94 # issue #166 - SDK library tooling

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