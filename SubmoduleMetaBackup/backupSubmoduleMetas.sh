# Backup all meta files inside all of your /Assets git submodules into ./MetaBackup directory
# Restore with contents in --BackupList.txt
mkdir -p MetaBackup; rm ./MetaBackup/*;  git submodule | grep -Eo "Assets.* "| while read line; do ls -R -1 -d $line/*.meta; done | tee ./MetaBackup/--BackupList.txt | while read line; do cp $line ./MetaBackup/;done
