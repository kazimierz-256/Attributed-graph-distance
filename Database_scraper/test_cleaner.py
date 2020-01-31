import unittest
import cleaner

param_list = [
    ["lorraine.lindberg@enron.</b>", "lorraine.lindberg@enron"],
]

class TestCleaner(unittest.TestCase):
    def test_cleaner_clean1(self):
        for email, cleaned_email in param_list:
            with self.subTest():
                cleaned_email_automatically = cleaner.fix_email(email)
                self.assertEqual(cleaned_email, cleaned_email_automatically)

        
if __name__ == '__main__':
    unittest.main()