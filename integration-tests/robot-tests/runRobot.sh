robot --listener "SLListener.py:${SL_TOKEN}::Robot Tests selenium:${SL_LAB_ID}" ./selenium.robot
#sleep 20
#robot --listener "SLListener.py:${SL_TOKEN}::Robot Tests-2:${SL_LAB_ID}" api_tests_2.robot



#sl-python start --labid ${SL_LAB_ID} --token ${SL_TOKEN} --teststage "Robot Tests" #add token and labid
#robot -xunit ./api_tests.robot
#sl-python uploadreports --reportfile "unit.xml" --labid ${SL_LAB_ID} --token ${SL_TOKEN} 
#sl-python end --labid ${SL_LAB_ID} --token ${SL_TOKEN}
