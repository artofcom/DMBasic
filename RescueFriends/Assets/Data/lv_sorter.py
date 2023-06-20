import os, json, sys

ep_cnt = [
    [10, 10, 14], 
    [14, 16, 16, 15],
    [15, 15, 18],
    [12, 13, 14, 15],
    [15, 14, 16], 
    [15, 15, 15, 15],
    [15, 15, 15],
    [14, 16, 15, 16],
    [15, 16, 16],
    [16, 14, 15],
]

theme_names = [
    'grassland',
    'forestarea',
    'rockfield',
    'yellowdesert',
    'antarctica',
    'darkantarctica',
    'tropicalisland',
    'deepocean',
    'jewelmine',
    'darkjewelmine'
]

buf = open(sys.argv[1], 'r').read()
data = json.loads(buf)

theme = 0
eps = 0
lvidx = 0

epobj = []

for lv in data:
    print theme, eps, lvidx
    lv['episodeIdx'] = eps
    lv['Index'] = lvidx
    epobj.append(lv)
    lvidx += 1
    if(lvidx == ep_cnt[theme][eps]):
        eps +=1
        lvidx = 0
        if(eps == len(ep_cnt[theme])):
            print 'saving '+theme_names[theme]
            eps = 0
            with open('lv_'+theme_names[theme]+'.txt', 'w') as outfile:
                json.dump(epobj, outfile)
            epobj = []
            theme +=1
