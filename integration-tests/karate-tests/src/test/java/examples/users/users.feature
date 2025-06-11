Feature: Tests
  Background: 
    # Fetch the machine DNS/IP from an environment variable
    * url machine_dns

  Scenario: Verify the API returns a successful response
    Given path '/'
    When method get
    Then status 200

  Scenario: Change currency
    Given path '/setCurrency'
    And form field currency_code = 'JPY'
    When method post
    Then status 200

  Scenario: Add item to cart
    Given path '/cart'
    And form field product_id = '1YMWWN1N4O'
    And form field quantity = '2'
    When method post
    Then status 200

  Scenario: Get cart
    Given path 'cart'
    When method get
    Then status 200