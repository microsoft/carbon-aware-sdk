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
git cherry-pick bb3c82d9823b0034bfc0ed67a5b4a9d53aece8c4 # issue #161 - new data source interfaces
git cherry-pick 381a377a25059e7e7974a476d976c8fa4cb56a66 # issue #161 - data source interfaces in config
git cherry-pick 7844751d0dab99459f74e133e65b45aa0342a19a # issue #160 - C# library
git cherry-pick 7c3df3e96204b5458449655ac745879223163667 # issue #166 - SDK library tooling
git cherry-pick 7a65f6094189def0506f22ddebf663daf91425f2 # issue #164 - New DataSource Config Schema
git cherry-pick 3e1aa3dc490e9a88e4b99956296d4443fb561fc2 # issue #195 - Bug fix for location string localization

####### Adding/updating features with this script:
####### 1) switch to your feature branch
#
# git switch <###/your-feature-branch>
#
#######
####### 2) Update your branch with the latest `release` commits
#
# git pull --rebase origin release
#
#######
####### 2a) Resolve any conflicts.
#######
####### 3) Squash your feature into a single commit
#
# git rebase -i HEAD~<number-of-commits-in-your-feature>  # EG: git rebase -i HEAD~3
#
#######
####### In the interactive window, squash your those commits into a single commit
####### follows the naming convention:
####### [M#][Issue#] Feature name
#######
####### 4) Push your feature branch to the remote
#
# git push --force-with-lease
#
####### 5) Copy your commit hashes
#
# git log --pretty=format:'git cherry-pick %H # %s' gsf/dev..HEAD | Set-Clipboard
#
#######
####### 6) Pull `dev` and branch off it
#
# git switch dev
# git pull
# git checkout -b release-update-<YYYY-MM-DD>
#
####### 7) Replace the `git cherry-pick` section of BOTH `create_release_candidate` scripts
####### If you used the `git log` command above, it will already be in your clipboard :)
#######
####### NOTE: All commit hashes will likely change from the original
#######       script, due to the nature of rebasing our commits.
#######
####### 8) Push and PR into `dev`
####### Dont worry about filling out the template.
####### Assign PR to the current Release Master
####### Tag the rest of the team for review