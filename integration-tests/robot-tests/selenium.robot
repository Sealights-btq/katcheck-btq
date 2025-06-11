*** Settings ***
Library           RequestsLibrary
Library           Collections
Library           SeleniumLibrary
Suite Setup       Set Suite Variable    ${BASE_URL}    %{machine_dns}
Suite Teardown    Close All Browsers
*** Variables ***
${load}    1
*** Test Cases ***
Scenario: Server live
    When the user opens the website
    Then the user gets homepage

Scenario: Hompage correct
    When the user visits website at /
    Then the user should see Hot Products

Scenario: Product correct
    When the user visits website at /product/OLJCESPC7Z
    Then the user should see button Add To Cart
*** Keywords ***
Open Headless Chrome
    [Documentation]    Opens Chrome in headless mode.
    ${options}=    Evaluate    sys.modules['selenium.webdriver'].ChromeOptions()    sys, selenium.webdriver
    Call Method    ${options}    add_argument    --headless
    Call Method    ${options}    add_argument    --no-sandbox
    Call Method    ${options}    add_argument    --disable-dev-shm-usage
    Call Method    ${options}    add_argument    --disable-gpu
    Create WebDriver    Chrome   options=${options}

When the user opens the website
    Create Session    frontend    ${BASE_URL}

Then the user gets homepage
    ${response}=    GET On Session    frontend    /
    Should Be Equal As Integers    ${response.status_code}    200

When the user visits website at ${path}
    Open Headless Chrome
    Go to    ${BASE_URL}${path} 

Then the user should see ${something}
    Wait Until Element Is Visible    xpath://h3[contains(text(),'Hot Products')]
    #Close Browser

Then the user should see button ${something}
    Wait Until Element Is Visible    xpath://button[contains(text(),'Add To Cart')]
    