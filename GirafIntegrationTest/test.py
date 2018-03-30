from testLib import *
from accountControllerTest import *
from roleControllerTest import *
import time
import sys
import json

# Nice error message if the server is down.
# First time I encountered the exception it took me 20 minutes to figure out why.
try:
    result = controllerTest('').request('GET', '/user')
    result['success']
except:
    print('Could not get response from server. \n'
          'Exiting...\n')
    sys.exit()

# Run ALL the tests!
#testAccountController()
testRoleController()

if controllerTest.testsFailed == 0:
    print '{0} tests were run. All tests passed.'.format(controllerTest.testsRun)
else:
    # Gotta be gramtically correct
    pluralS = 's'
    if controllerTest.testsFailed == 1:
        pluralS = ''

    print ('{0} test{1} failed out of {2} tests run. Happy debugging.'
           .format(controllerTest.testsFailed, pluralS, controllerTest.testsRun))
